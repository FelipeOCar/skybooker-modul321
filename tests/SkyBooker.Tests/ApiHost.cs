using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Controllers;
using SkyBooker.AuthService.Data;
using SkyBooker.AuthService.Models;
using SkyBooker.AuthService.Services;
using SkyBooker.AuthService.Validation;
using SkyBooker.FlightService.Contracts;
using SkyBooker.FlightService.Controllers;
using SkyBooker.FlightService.Data;
using SkyBooker.FlightService.Services;
using SkyBooker.FlightService.Validation;
using SkyBooker.BookingService.Contracts;
using SkyBooker.BookingService.Controllers;
using SkyBooker.BookingService.Data;
using SkyBooker.BookingService.Services;
using SkyBooker.BookingService.Validation;
using SkyBooker.Shared;

namespace SkyBooker.Tests;

public sealed class ApiHost : IAsyncLifetime
{
    private WebApplication _app = null!;
    private readonly SqliteConnection _auth = new("Data Source=:memory:");
    private readonly SqliteConnection _booking = new("Data Source=:memory:");
    public HttpClient Client { get; private set; } = null!;
    public string Key { get; private set; } = "";
    public async Task InitializeAsync()
    {
        await _auth.OpenAsync(); await _booking.OpenAsync();
        var b = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        b.Configuration.AddInMemoryCollection(new Dictionary<string,string?>
        { ["Jwt:Key"] = new('t',88), ["Jwt:Issuer"] = "SkyBooker.Auth", ["Jwt:Audience"] = "SkyBooker.Services" });
        b.AddSkyBooker("IntegrationTests");
        Key = b.Configuration["Jwt:Key"]!;
        b.WebHost.UseTestServer();
        b.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly)
            .AddApplicationPart(typeof(FlightController).Assembly).AddApplicationPart(typeof(BookingController).Assembly);
        b.Services.AddDbContext<AuthDbContext>(o => o.UseSqlite(_auth));
        b.Services.AddDbContext<BookingDbContext>(o => o.UseSqlite(_booking));
        b.Services.AddDbContext<FlightDbContext>(o => o.UseInMemoryDatabase("flights-"+Key.GetHashCode()));
        b.Services.AddScoped<IPasswordHasher<User>,PasswordHasher<User>>();
        b.Services.AddScoped<AuthManager>(); b.Services.AddScoped<TokenService>(); b.Services.AddScoped<FlightManager>(); b.Services.AddScoped<BookingManager>();
        b.Services.AddSingleton<IValidator<RegisterRequest>,RegisterValidator>(); b.Services.AddSingleton<IValidator<LoginRequest>,LoginValidator>();
        b.Services.AddSingleton<IValidator<CreateFlightRequest>,FlightValidator>(); b.Services.AddSingleton<IValidator<CreateBookingRequest>,BookingValidator>();
        // Real typed HTTP client and JWT forwarding, routed to the same TestServer's FlightController.
        b.Services.AddScoped<IFlightClient>(sp => new FlightClient(_app.GetTestClient(), TimeProvider.System, sp.GetRequiredService<ILogger<FlightClient>>()));
        _app = b.Build();
        using (var scope=_app.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.EnsureCreatedAsync();
            await scope.ServiceProvider.GetRequiredService<BookingDbContext>().Database.EnsureCreatedAsync();
        }
        _app.UseSkyBooker();
        await _app.StartAsync();
        Client=_app.GetTestClient();
    }
    public async Task DisposeAsync()
    {
        Client.Dispose(); await _app.DisposeAsync(); await _auth.DisposeAsync(); await _booking.DisposeAsync();
    }
}
