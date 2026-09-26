namespace SkyBooker.BookingService.Models;

public sealed class Booking
{
    public int Id { get; set; }
    public string FlightId { get; set; } = "";
    public int PassengerId { get; set; }
    public string PassengerFirstname { get; set; } = "";
    public string PassengerLastname { get; set; } = "";
    public int TicketCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
