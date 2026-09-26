using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Data;
using SkyBooker.AuthService.Models;
using SkyBooker.AuthService.Services;
using SkyBooker.AuthService.Validation;
using SkyBooker.Shared;

var builder = WebApplication.CreateBuilder(args).AddSkyBooker("AuthService");
string? connection = builder.Configuration.GetConnectionString("AuthDb");
if (string.IsNullOrEmpty(connection))
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "data"));
    connection = $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "data", "auth.db")}";
}
builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlite(connection));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddSingleton<IValidator<RegisterRequest>, RegisterValidator>();
builder.Services.AddSingleton<IValidator<LoginRequest>, LoginValidator>();
var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreatedAsync();
app.UseSkyBooker();
app.MapGet("/health/ready", async (AuthDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503)).AllowAnonymous();
app.Run();
public partial class Program { }
