using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Services;
using SkyBooker.Shared;

namespace SkyBooker.AuthService.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public sealed class AuthController(AuthManager manager, IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<UserResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var validation = await registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid) return this.Invalid(validation);
        var user = await manager.RegisterAsync(request, ct);
        if (user is null) return Problem(statusCode: 409, title: "Benutzername oder E-Mail bereits registriert.");
        logger.LogInformation("UserRegistered. TraceId {TraceId}", HttpContext.TraceIdentifier);
        return StatusCode(201, user);
    }

    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(200)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var validation = await loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid) return this.Invalid(validation);
        var token = await manager.LoginAsync(request, ct);
        Response.Headers.CacheControl = "no-store";
        if (token is null) return Unauthorized(new ProblemDetails { Status = 401, Title = "Benutzername oder Passwort falsch." });
        logger.LogInformation("LoginSucceeded. TraceId {TraceId}", HttpContext.TraceIdentifier);
        return Ok(token);
    }

    // Required GET route. Credentials go in HTTP Basic, never in a URL or query string.
    [HttpGet("login")]
    [ProducesResponseType<TokenResponse>(200)]
    [ProducesResponseType(401)]
    public Task<ActionResult<TokenResponse>> LoginGet(CancellationToken ct)
    {
        string header = Request.Headers.Authorization.ToString();
        if (header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header[6..].Trim()));
                int colon = decoded.IndexOf(':');
                if (colon > 0) return Login(new(decoded[..colon], decoded[(colon + 1)..]), ct);
            }
            catch (FormatException) { }
        }
        Response.Headers.WWWAuthenticate = "Basic realm=\"SkyBooker\"";
        Response.Headers.CacheControl = "no-store";
        return Task.FromResult<ActionResult<TokenResponse>>(Unauthorized());
    }
}
