using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using SkyBooker.FlightService.Contracts;
using SkyBooker.FlightService.Data;
using SkyBooker.FlightService.Services;
using SkyBooker.FlightService.Validation;
using SkyBooker.Shared;

var builder = WebApplication.CreateBuilder(args).AddSkyBooker("FlightService");
string connection = builder.Configuration.GetConnectionString("FlightDb") ?? throw new InvalidOperationException("ConnectionStrings:FlightDb fehlt. Setup.cmd ausführen.");
string database = builder.Configuration["Mongo:Database"] ?? "SkyBookerFlights";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connection));
builder.Services.AddDbContext<FlightDbContext>((services, options) => options.UseMongoDB(services.GetRequiredService<IMongoClient>(), database));
builder.Services.AddScoped<FlightManager>();
builder.Services.AddSingleton<IValidator<CreateFlightRequest>, FlightValidator>();
var app = builder.Build();
if (!app.Environment.IsEnvironment("Testing"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<FlightDbContext>().Database.EnsureCreatedAsync();
    // Driver used only for database index administration. Entity operations use EF Core.
    var collection = scope.ServiceProvider.GetRequiredService<IMongoClient>().GetDatabase(database).GetCollection<BsonDocument>("flights");
    await collection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("flightId"), new CreateIndexOptions { Unique = true }));
}
app.UseSkyBooker();
app.MapGet("/health/ready", async (IMongoClient client, CancellationToken ct) =>
{
    try { await client.GetDatabase(database).RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: ct); return Results.Ok(new { status = "ready" }); }
    catch (MongoException) { return Results.StatusCode(503); }
}).AllowAnonymous();
app.Run();
public partial class Program { }
