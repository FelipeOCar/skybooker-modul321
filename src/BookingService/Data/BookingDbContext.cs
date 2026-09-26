using Microsoft.EntityFrameworkCore;
using SkyBooker.BookingService.Models;

namespace SkyBooker.BookingService.Data;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Booking> Bookings => Set<Booking>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var booking = model.Entity<Booking>();
        booking.ToTable("Booking", table => table.HasCheckConstraint("CK_Booking_TicketCount", "[TicketCount] > 0 AND [TicketCount] <= 1000"));
        booking.HasKey(x => x.Id);
        booking.Property(x => x.Id).ValueGeneratedOnAdd();
        booking.Property(x => x.FlightId).HasMaxLength(12).IsRequired();
        booking.Property(x => x.PassengerFirstname).HasMaxLength(100).IsRequired();
        booking.Property(x => x.PassengerLastname).HasMaxLength(100).IsRequired();
        booking.Property(x => x.CreatedAt).HasColumnType("datetime2");
        booking.Property(x => x.UpdatedAt).HasColumnType("datetime2");
        booking.HasIndex(x => x.PassengerId);
    }
}
