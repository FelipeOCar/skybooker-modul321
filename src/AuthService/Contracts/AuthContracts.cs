namespace SkyBooker.AuthService.Contracts;

public sealed record RegisterRequest(string Username, string EMail, string Password);
public sealed record LoginRequest(string Username, string Password);
public sealed record UserResponse(int Id, string Username, string EMail);
public sealed record TokenResponse(string Token, string TokenType, int ExpiresInSeconds, int PassengerId, string Username);
