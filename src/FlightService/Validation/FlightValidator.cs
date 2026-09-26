using FluentValidation;
using SkyBooker.FlightService.Contracts;

namespace SkyBooker.FlightService.Validation;

public sealed class FlightValidator : AbstractValidator<CreateFlightRequest>
{
    public FlightValidator(TimeProvider clock)
    {
        RuleFor(x => x.FlightId).NotEmpty().Matches("^[A-Z0-9]{2,12}$");
        RuleFor(x => x.AirlineName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(100)
            .Must((x, destination) => !string.Equals(x.Source?.Trim(), destination?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Abflugort und Ziel müssen verschieden sein.");
        RuleFor(x => x.DepartureTime).Must(value => value > clock.GetUtcNow()).WithMessage("Abflug muss in der Zukunft liegen.");
        RuleFor(x => x.ArrivalTime).GreaterThan(x => x.DepartureTime);
        RuleFor(x => x.AvailableSeats).InclusiveBetween(0, 1000);
    }
}
