using SkyBooker.AuthService.Contracts;
using SkyBooker.AuthService.Validation;
using SkyBooker.FlightService.Contracts;
using SkyBooker.FlightService.Validation;
using SkyBooker.BookingService.Contracts;
using SkyBooker.BookingService.Validation;

namespace SkyBooker.Tests;

public sealed class ValidationTests
{
    [Theory]
    [InlineData("kurz")]
    [InlineData("onlylowercase123!")]
    [InlineData("ONLYUPPERCASE123!")]
    [InlineData("NoDigitsButLong!")]
    [InlineData("NoSpecialBut1234")]
    public void RegistrationRejectsWeakPasswords(string password) =>
        Assert.False(new RegisterValidator().Validate(new("felipe", "felipe@example.com", password)).IsValid);

    [Theory]
    [InlineData("a", "valid@example.com")]
    [InlineData("abc def", "valid@example.com")]
    [InlineData("valid", "wrong")]
    public void RegistrationRejectsInvalidIdentity(string username, string email) =>
        Assert.False(new RegisterValidator().Validate(new(username, email, "SkyBooker!2026")).IsValid);

    [Fact]
    public void ValidRegistrationAccepted() => Assert.True(new RegisterValidator().Validate(new("felipe", "felipe@example.com", "SkyBooker!2026")).IsValid);

    private static CreateFlightRequest Flight => new("LX123", "SWISS", "Basel", "London", DateTimeOffset.UtcNow.AddDays(3), DateTimeOffset.UtcNow.AddDays(3).AddHours(2), 100);

    [Fact]
    public void FlightRejectsSameAirport() => Assert.False(new FlightValidator(TimeProvider.System).Validate(Flight with { Destination = "BASEL" }).IsValid);
    [Fact]
    public void FlightRejectsArrivalBeforeDeparture() => Assert.False(new FlightValidator(TimeProvider.System).Validate(Flight with { ArrivalTime = DateTimeOffset.UtcNow }).IsValid);
    [Fact]
    public void FlightRejectsPastDeparture() => Assert.False(new FlightValidator(TimeProvider.System).Validate(Flight with { DepartureTime = DateTimeOffset.UtcNow.AddDays(-1) }).IsValid);
    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void FlightRejectsInvalidCapacity(int seats) => Assert.False(new FlightValidator(TimeProvider.System).Validate(Flight with { AvailableSeats = seats }).IsValid);
    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public void FlightAcceptsBoundaryCapacity(int seats) => Assert.True(new FlightValidator(TimeProvider.System).Validate(Flight with { AvailableSeats = seats }).IsValid);
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void BookingRejectsInvalidTicketCount(int tickets) => Assert.False(new BookingValidator().Validate(new("LX123", 1, "Felipe", "Carvalho", tickets)).IsValid);
}
