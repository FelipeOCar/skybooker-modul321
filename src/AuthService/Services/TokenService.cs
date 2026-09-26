using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Models;

namespace SkyBooker.AuthService.Services;

public sealed class TokenService(IConfiguration configuration, TimeProvider clock)
{
    public TokenResponse Create(User user)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = configuration["Jwt:Issuer"], Audience = configuration["Jwt:Audience"],
            Subject = new ClaimsIdentity([new Claim("sub", user.Id.ToString(CultureInfo.InvariantCulture)), new Claim("name", user.Username)]),
            IssuedAt = now, NotBefore = now, Expires = now.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)), SecurityAlgorithms.HmacSha512)
        };
        var handler = new JwtSecurityTokenHandler();
        return new TokenResponse(handler.WriteToken(handler.CreateToken(descriptor)), "Bearer", 900, user.Id, user.Username);
    }
}
