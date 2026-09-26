using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Data;
using SkyBooker.AuthService.Models;
using SkyBooker.AuthService.Services;

namespace SkyBooker.Tests;

public sealed class AuthManagerTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AuthDbContext _db;
    private readonly AuthManager _manager;
    public AuthManagerTests()
    {
        _connection.Open();
        _db = new(new DbContextOptionsBuilder<AuthDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
            { ["Jwt:Key"] = new('k', 88), ["Jwt:Issuer"] = "SkyBooker.Auth", ["Jwt:Audience"] = "SkyBooker.Services" }).Build();
        _manager = new(_db, new PasswordHasher<User>(), new TokenService(config, TimeProvider.System));
    }
    [Fact]
    public async Task RegistrationHashesPasswordAndRejectsDuplicatesIgnoringCase()
    {
        var input = new RegisterRequest("Felipe", "felipe@example.com", "SkyBooker!2026");
        var registered = await _manager.RegisterAsync(input, default);
        Assert.NotNull(registered);
        var user = await _db.Users.SingleAsync();
        Assert.NotEqual(input.Password, user.Password);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, input.Password));
        Assert.Null(await _manager.RegisterAsync(input with { Username = "FELIPE", EMail = "other@example.com" }, default));
        Assert.Null(await _manager.RegisterAsync(input with { Username = "other", EMail = "FELIPE@EXAMPLE.COM" }, default));
    }
    [Fact]
    public async Task WrongPasswordNeverIssuesToken()
    {
        await _manager.RegisterAsync(new("felipe", "f@example.com", "SkyBooker!2026"), default);
        Assert.Null(await _manager.LoginAsync(new("felipe", "incorrect"), default));
        Assert.Null(await _manager.LoginAsync(new("unknown", "SkyBooker!2026"), default));
    }
    [Fact]
    public async Task TokenContainsStablePassengerIdAndExpiresAfterFifteenMinutes()
    {
        var user = await _manager.RegisterAsync(new("felipe", "f@example.com", "SkyBooker!2026"), default);
        var token = await _manager.LoginAsync(new("felipe", "SkyBooker!2026"), default);
        Assert.NotNull(token);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);
        Assert.Equal(user!.Id.ToString(), jwt.Subject);
        Assert.Equal("HS512", jwt.Header.Alg);
        Assert.Equal(TimeSpan.FromMinutes(15), jwt.ValidTo - jwt.ValidFrom);
        Assert.Equal("SkyBooker.Auth", jwt.Issuer);
    }
    public void Dispose() { _db.Dispose(); _connection.Dispose(); }
}
