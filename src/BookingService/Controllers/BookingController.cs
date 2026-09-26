using System.Globalization;
using System.Net.Http.Headers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBooker.BookingService.Contracts;
using SkyBooker.BookingService.Models;
using SkyBooker.BookingService.Services;
using SkyBooker.Shared;

namespace SkyBooker.BookingService.Controllers;

[ApiController]
[Authorize]
[Route("api/booking")]
public sealed class BookingController(BookingManager manager, IValidator<CreateBookingRequest> validator,
    ILogger<BookingController> logger) : ControllerBase
{
    private int? PassengerId => int.TryParse(User.FindFirst("sub")?.Value, NumberStyles.None,
        CultureInfo.InvariantCulture, out int id) && id > 0 ? id : null;

    [HttpGet]
    [ProducesResponseType<List<Booking>>(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<Booking>>> GetAll(CancellationToken ct)
    {
        if (PassengerId is not { } passengerId) return Unauthorized();
        Response.Headers.CacheControl = "no-store";
        return Ok(await manager.GetAllAsync(passengerId, ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Booking>(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Booking>> Get(int id, CancellationToken ct)
    {
        if (PassengerId is not { } passengerId) return Unauthorized();
        Response.Headers.CacheControl = "no-store";
        var booking = await manager.GetAsync(id, passengerId, ct);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost]
    [ProducesResponseType<Booking>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType<ProblemDetails>(422)]
    [ProducesResponseType<ProblemDetails>(503)]
    public async Task<ActionResult<Booking>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        if (PassengerId is not { } passengerId) return Unauthorized();
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return this.Invalid(validation);
        if (request.PassengerId != passengerId) return Forbid();
        var header = AuthenticationHeaderValue.Parse(Request.Headers.Authorization.ToString());
        var result = await manager.CreateAsync(request, header.Parameter!, ct);
        if (result.FlightStatus == FlightCheck.NotFound) return Problem(statusCode: 422, title: "Flugnummer existiert nicht.");
        if (result.FlightStatus == FlightCheck.Departed) return Problem(statusCode: 422, title: "Flug ist bereits abgeflogen.");
        if (result.FlightStatus != FlightCheck.Found) return Problem(statusCode: 503, title: "FlightService momentan nicht erreichbar. Es wurde keine Buchung gespeichert.");
        logger.LogInformation("BookingCreated {BookingId} for {FlightId}", result.Booking!.Id, result.Booking.FlightId);
        return CreatedAtAction(nameof(Get), new { id = result.Booking.Id }, result.Booking);
    }
}
