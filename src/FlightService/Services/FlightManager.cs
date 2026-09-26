using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using SkyBooker.FlightService.Contracts;
using SkyBooker.FlightService.Data;
using SkyBooker.FlightService.Models;

namespace SkyBooker.FlightService.Services;

public sealed class FlightManager(FlightDbContext db, TimeProvider clock)
{
    public Task<List<Flight>> GetAllAsync(CancellationToken ct) => db.Flights.AsNoTracking().OrderBy(x => x.DepartureTime).ToListAsync(ct);
    public Task<Flight?> GetAsync(ObjectId id, CancellationToken ct) => db.Flights.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Flight?> FindNumberAsync(string number, CancellationToken ct) => db.Flights.AsNoTracking().SingleOrDefaultAsync(x => x.FlightId == number, ct);
    public async Task<Flight?> CreateAsync(CreateFlightRequest input, CancellationToken ct)
    {
        if (await db.Flights.AnyAsync(x => x.FlightId == input.FlightId, ct)) return null;
        var now = clock.GetUtcNow().UtcDateTime;
        var flight = new Flight { Id = ObjectId.GenerateNewId(), FlightId = input.FlightId,
            AirlineName = input.AirlineName.Trim(), Source = input.Source.Trim(), Destination = input.Destination.Trim(),
            DepartureTime = input.DepartureTime.UtcDateTime, ArrivalTime = input.ArrivalTime.UtcDateTime,
            AvailableSeats = input.AvailableSeats, CreatedAt = now, UpdatedAt = now };
        db.Flights.Add(flight);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (await db.Flights.AnyAsync(x => x.FlightId == input.FlightId, ct)) return null;
            throw;
        }
        return flight;
    }
}
