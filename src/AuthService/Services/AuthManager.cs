using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Data;
using SkyBooker.AuthService.Models;

namespace SkyBooker.AuthService.Services;

public sealed class AuthManager(AuthDbContext db, IPasswordHasher<User> hasher, TokenService tokens)
{
    public async Task<UserResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        string username = request.Username.Trim();
        string email = request.EMail.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Username == username || u.EMail == email, ct)) return null;
        var user = new User { Username = username, EMail = email };
        user.Password = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) // The unique database indexes also close concurrent-registration races.
        {
            db.ChangeTracker.Clear();
            if (await db.Users.AnyAsync(u => u.Username == username || u.EMail == email, ct)) return null;
            throw;
        }
        return new(user.Id, user.Username, user.EMail);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == request.Username.Trim(), ct);
        if (user is null) return null;
        var result = hasher.VerifyHashedPassword(user, user.Password, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = hasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(ct);
        }
        return tokens.Create(user);
    }
}
