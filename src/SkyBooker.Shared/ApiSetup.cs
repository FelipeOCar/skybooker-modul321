using System.Text;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace SkyBooker.Shared;

public static class ApiSetup
{
    public static WebApplicationBuilder AddSkyBooker(this WebApplicationBuilder builder, string serviceName)
    {
        LocalConfiguration.Load(builder.Configuration);
        builder.Host.UseSerilog((context, services, log) => log
            .MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext().Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console(new RenderedCompactJsonFormatter()));
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = $"SkyBooker {serviceName}", Version = "v1",
                Description = "Modul 321. Projekt: Felipe Carvalho. Zeitangaben in UTC." });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http,
                Scheme = "bearer", BearerFormat = "JWT", Description = "JWT einfügen, ohne das Wort Bearer." });
            options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http,
                Scheme = "basic", Description = "Nur GET /api/login. Benutzername und Passwort." });
            options.OperationFilter<SwaggerSecurityFilter>();
        });
        string key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT-Konfiguration fehlt. Im Solution-Ordner zuerst Setup.cmd ausführen.");
        if (Encoding.UTF8.GetByteCount(key) < 64) throw new InvalidOperationException("Jwt:Key muss mindestens 64 UTF-8-Bytes enthalten.");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero, ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha512 },
                NameClaimType = "name"
            };
        });
        builder.Services.AddAuthorization();
        return builder;
    }

    public static WebApplication UseSkyBooker(this WebApplication app)
    {
        // Request bodies, query strings, passwords and Authorization headers are never logged.
        app.UseSerilogRequestLogging(options => options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0000} ms");
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
        if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
        app.MapControllers();
        app.MapGet("/health/live", () => Results.Ok(new { status = "live" })).AllowAnonymous();
        return app;
    }

    public static ActionResult Invalid(this ControllerBase controller, ValidationResult result) => controller.ValidationProblem(
        new ValidationProblemDetails(result.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
}

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Log only type and trace id; database exceptions may contain submitted personal data.
        logger.LogError("Unhandled {ExceptionType}. TraceId {TraceId}", exception.GetType().Name, context.TraceIdentifier);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = 500,
            Title = "Interner Fehler", Detail = "Der Aufruf konnte nicht verarbeitet werden.",
            Extensions = { ["traceId"] = context.TraceIdentifier } }, cancellationToken);
        return true;
    }
}
