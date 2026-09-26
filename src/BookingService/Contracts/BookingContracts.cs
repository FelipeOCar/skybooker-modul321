namespace SkyBooker.BookingService.Contracts;

public sealed record CreateBookingRequest(string FlightId, int PassengerId,
    string PassengerFirstname, string PassengerLastname, int TicketCount);
