using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using SkyBooker.FlightService.Contracts;
using SkyBooker.FlightService.Services;
using SkyBooker.Shared;

namespace SkyBooker.FlightService.Controllers;

[ApiController]
[Authorize]
[Route("api/flight")]
public sealed class FlightController(FlightManager manager, IValidator<CreateFlightRequest> validator,
    ILogger<FlightController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<FlightResponse>>(200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<FlightResponse>>> GetAll(CancellationToken ct) =>
        Ok((await manager.GetAllAsync(ct)).Select(FlightResponse.From).ToList());

    [HttpGet("{id}")]
    [ProducesResponseType<FlightResponse>(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FlightResponse>> Get(string id, CancellationToken ct)
    {
        if (!ObjectId.TryParse(id, out var objectId)) return Problem(statusCode: 400, title: "Ungültige Flug-ID.");
        var flight = await manager.GetAsync(objectId, ct);
        return flight is null ? NotFound() : Ok(FlightResponse.From(flight));
    }

    [HttpGet("by-number/{flightId}")]
    [ProducesResponseType<FlightResponse>(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FlightResponse>> GetByNumber(string flightId, CancellationToken ct)
    {
        var flight = await manager.FindNumberAsync(flightId, ct);
        return flight is null ? NotFound() : Ok(FlightResponse.From(flight));
    }

    [HttpPost]
    [ProducesResponseType<FlightResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<FlightResponse>> Create(CreateFlightRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return this.Invalid(result);
        var flight = await manager.CreateAsync(request, ct);
        if (flight is null) return Problem(statusCode: 409, title: "Flugnummer existiert bereits.");
        logger.LogInformation("FlightCreated {FlightId}", flight.FlightId);
        return CreatedAtAction(nameof(Get), new { id = flight.Id.ToString() }, FlightResponse.From(flight));
    }
}
