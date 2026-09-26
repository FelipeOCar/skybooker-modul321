using FluentValidation;
using SkyBooker.BookingService.Contracts;

namespace SkyBooker.BookingService.Validation;

public sealed class BookingValidator : AbstractValidator<CreateBookingRequest>
{
    public BookingValidator()
    {
        RuleFor(x => x.FlightId).NotEmpty().Matches("^[A-Z0-9]{2,12}$");
        RuleFor(x => x.PassengerId).GreaterThan(0);
        RuleFor(x => x.PassengerFirstname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PassengerLastname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TicketCount).InclusiveBetween(1, 1000);
    }
}
