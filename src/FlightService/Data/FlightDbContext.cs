using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using SkyBooker.FlightService.Models;

namespace SkyBooker.FlightService.Data;

public sealed class FlightDbContext : DbContext
{
    public FlightDbContext(DbContextOptions<FlightDbContext> options) : base(options)
    {
        // Each operation writes a single document. This also supports a local standalone MongoDB.
        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
    public DbSet<Flight> Flights => Set<Flight>();
    protected override void OnModelCreating(ModelBuilder model) => model.Entity<Flight>().ToCollection("flights");
}
