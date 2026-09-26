using System.Text.Json.Serialization;
using SkyBooker.FlightService.Models;

namespace SkyBooker.FlightService.Contracts;

public sealed record CreateFlightRequest(string FlightId, string AirlineName, string Source, string Destination,
    [property: JsonPropertyName("departure_time")] DateTimeOffset DepartureTime,
    [property: JsonPropertyName("arrival_time")] DateTimeOffset ArrivalTime,
    [property: JsonPropertyName("available_seats")] int AvailableSeats);

public sealed record FlightResponse(string Id, string FlightId, string AirlineName, string Source, string Destination,
    [property: JsonPropertyName("departure_time")] DateTime DepartureTime,
    [property: JsonPropertyName("arrival_time")] DateTime ArrivalTime,
    [property: JsonPropertyName("available_seats")] int AvailableSeats,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt)
{
    public static FlightResponse From(Flight x) => new(x.Id.ToString(), x.FlightId, x.AirlineName, x.Source,
        x.Destination, x.DepartureTime, x.ArrivalTime, x.AvailableSeats, x.CreatedAt, x.UpdatedAt);
}
