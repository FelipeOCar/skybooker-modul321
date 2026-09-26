using FluentValidation;
using SkyBooker.AuthService.Contracts;

namespace SkyBooker.AuthService.Validation;

public sealed class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username).NotEmpty().Length(3, 50).Matches("^[A-Za-z0-9_.-]+$");
        RuleFor(x => x.EMail).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().Length(12, 128)
            .Matches("[A-Z]").WithMessage("Mindestens ein Grossbuchstabe erforderlich.")
            .Matches("[a-z]").WithMessage("Mindestens ein Kleinbuchstabe erforderlich.")
            .Matches("[0-9]").WithMessage("Mindestens eine Ziffer erforderlich.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Mindestens ein Sonderzeichen erforderlich.");
    }
}

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
