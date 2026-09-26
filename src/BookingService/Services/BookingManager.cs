using Microsoft.EntityFrameworkCore;
using SkyBooker.BookingService.Contracts;
using SkyBooker.BookingService.Data;
using SkyBooker.BookingService.Models;

namespace SkyBooker.BookingService.Services;

public sealed record BookingResult(FlightCheck FlightStatus, Booking? Booking);

public sealed class BookingManager(BookingDbContext db, IFlightClient flights, TimeProvider clock)
{
    public Task<List<Booking>> GetAllAsync(int passengerId, CancellationToken ct) => db.Bookings.AsNoTracking()
        .Where(x => x.PassengerId == passengerId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    public Task<Booking?> GetAsync(int id, int passengerId, CancellationToken ct) => db.Bookings.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == id && x.PassengerId == passengerId, ct);
    public async Task<BookingResult> CreateAsync(CreateBookingRequest request, string jwt, CancellationToken ct)
    {
        var check = await flights.CheckAsync(request.FlightId, jwt, ct);
        if (check != FlightCheck.Found) return new(check, null);
        var now = clock.GetUtcNow().UtcDateTime;
        var booking = new Booking { FlightId = request.FlightId, PassengerId = request.PassengerId,
            PassengerFirstname = request.PassengerFirstname.Trim(), PassengerLastname = request.PassengerLastname.Trim(),
            TicketCount = request.TicketCount, CreatedAt = now, UpdatedAt = now };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);
        return new(FlightCheck.Found, booking);
    }
}
