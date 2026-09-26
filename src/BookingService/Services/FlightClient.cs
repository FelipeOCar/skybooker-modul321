using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkyBooker.BookingService.Services;

public enum FlightCheck { Found, NotFound, Departed, Unavailable }
public interface IFlightClient
{
    Task<FlightCheck> CheckAsync(string flightId, string jwt, CancellationToken ct);
}

public sealed class FlightClient(HttpClient http, TimeProvider clock, ILogger<FlightClient> logger) : IFlightClient
{
    private sealed record FlightLookup(string FlightId, [property: JsonPropertyName("departure_time")] DateTimeOffset DepartureTime);

    public async Task<FlightCheck> CheckAsync(string flightId, string jwt, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/flight/by-number/" + Uri.EscapeDataString(flightId));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        try
        {
            using var response = await http.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return FlightCheck.NotFound;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("FlightLookupFailed {StatusCode}", (int)response.StatusCode);
                return FlightCheck.Unavailable;
            }
            var flight = await response.Content.ReadFromJsonAsync<FlightLookup>(cancellationToken: ct);
            if (flight is null || flight.FlightId != flightId) return FlightCheck.Unavailable;
            return flight.DepartureTime > clock.GetUtcNow() ? FlightCheck.Found : FlightCheck.Departed;
        }
        catch (HttpRequestException) { return FlightCheck.Unavailable; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return FlightCheck.Unavailable; }
        catch (JsonException) { return FlightCheck.Unavailable; }
    }
}
