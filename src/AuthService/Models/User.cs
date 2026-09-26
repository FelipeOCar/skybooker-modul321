namespace SkyBooker.AuthService.Models;

public sealed class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string EMail { get; set; } = "";
    // ASP.NET PasswordHasher format includes salt, algorithm version and iteration count.
    public string Password { get; set; } = "";
}
