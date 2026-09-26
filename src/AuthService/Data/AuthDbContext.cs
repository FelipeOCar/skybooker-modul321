using Microsoft.EntityFrameworkCore;
using SkyBooker.AuthService.Models;

namespace SkyBooker.AuthService.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var user = model.Entity<User>();
        user.ToTable("User");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).ValueGeneratedOnAdd();
        user.Property(x => x.Username).HasMaxLength(50).UseCollation("NOCASE").IsRequired();
        user.Property(x => x.EMail).HasMaxLength(254).UseCollation("NOCASE").IsRequired();
        user.Property(x => x.Password).HasMaxLength(512).IsRequired();
        user.HasIndex(x => x.Username).IsUnique();
        user.HasIndex(x => x.EMail).IsUnique();
    }
}
