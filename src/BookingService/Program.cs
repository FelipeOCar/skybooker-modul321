using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SkyBooker.BookingService.Contracts;
using SkyBooker.BookingService.Data;
using SkyBooker.BookingService.Services;
using SkyBooker.BookingService.Validation;
using SkyBooker.Shared;

var builder = WebApplication.CreateBuilder(args).AddSkyBooker("BookingService");
string connection = builder.Configuration.GetConnectionString("BookingDb") ?? throw new InvalidOperationException("ConnectionStrings:BookingDb fehlt. Setup.cmd ausführen.");
builder.Services.AddDbContext<BookingDbContext>(options => options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure(5)));
builder.Services.AddScoped<BookingManager>();
builder.Services.AddSingleton<IValidator<CreateBookingRequest>, BookingValidator>();
builder.Services.AddHttpClient<IFlightClient, FlightClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:FlightUrl"] ?? "http://127.0.0.1:32511/");
    client.Timeout = TimeSpan.FromSeconds(5);
});
var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<BookingDbContext>().Database.EnsureCreatedAsync();
app.UseSkyBooker();
app.MapGet("/health/ready", async (BookingDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503)).AllowAnonymous();
app.Run();
public partial class Program { }
