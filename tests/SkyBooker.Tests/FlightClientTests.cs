using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging.Abstractions;
using SkyBooker.BookingService.Services;

namespace SkyBooker.Tests;

public sealed class FlightClientTests
{
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(respond(request));
    }
    [Theory]
    [InlineData(404, FlightCheck.NotFound)]
    [InlineData(500, FlightCheck.Unavailable)]
    [InlineData(401, FlightCheck.Unavailable)]
    public async Task UpstreamFailuresAreMapped(int status, FlightCheck expected)
    {
        using var client = new HttpClient(new Handler(_ => new((HttpStatusCode)status))) { BaseAddress = new("http://flight/") };
        Assert.Equal(expected, await new FlightClient(client, TimeProvider.System, NullLogger<FlightClient>.Instance).CheckAsync("LX123", "token", default));
    }
    [Fact]
    public async Task ValidRequestForwardsBearerAndChecksDeparture()
    {
        using var client = new HttpClient(new Handler(request =>
        {
            Assert.Equal("http://flight/api/flight/by-number/LX123", request.RequestUri!.ToString());
            Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
            Assert.Equal("token", request.Headers.Authorization.Parameter);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new { flightId = "LX123", departure_time = DateTimeOffset.UtcNow.AddDays(1) }) };
        })) { BaseAddress = new("http://flight/") };
        Assert.Equal(FlightCheck.Found, await new FlightClient(client, TimeProvider.System, NullLogger<FlightClient>.Instance).CheckAsync("LX123", "token", default));
    }
    [Fact]
    public async Task DepartedFlightRejected()
    {
        using var client = new HttpClient(new Handler(_ => new(HttpStatusCode.OK) { Content = JsonContent.Create(new { flightId = "LX123", departure_time = DateTimeOffset.UtcNow.AddDays(-1) }) })) { BaseAddress = new("http://flight/") };
        Assert.Equal(FlightCheck.Departed, await new FlightClient(client, TimeProvider.System, NullLogger<FlightClient>.Instance).CheckAsync("LX123", "token", default));
    }
    [Fact]
    public async Task MalformedResponseNeverCreatesBooking()
    {
        using var client = new HttpClient(new Handler(_ => new(HttpStatusCode.OK) { Content = new StringContent("not json") })) { BaseAddress = new("http://flight/") };
        Assert.Equal(FlightCheck.Unavailable, await new FlightClient(client, TimeProvider.System, NullLogger<FlightClient>.Instance).CheckAsync("LX123", "token", default));
    }
}
