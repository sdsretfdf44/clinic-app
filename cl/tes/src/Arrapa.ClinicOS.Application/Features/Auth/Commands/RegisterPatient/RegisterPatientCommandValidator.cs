using FluentValidation;

namespace Arrapa.ClinicOS.Application.Features.Auth.Commands.RegisterPatient;

public sealed class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.TenantSlug)
            .NotEmpty().WithMessage("Please choose a clinic.")
            .MaximumLength(64)
            .Matches("^[a-z0-9-]+$").WithMessage("Tenant slug may only contain lowercase letters, digits, and hyphens.");

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30)
            .Matches(@"^\+?[0-9\s\-()]{7,}$").WithMessage("Phone number is not valid.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Password must be at least 10 characters.")
            .MaximumLength(200)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^A-Za-z0-9]").WithMessage("Password must contain a special character.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob is null || dob <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.VerificationUrlTemplate).NotEmpty();
    }
}
