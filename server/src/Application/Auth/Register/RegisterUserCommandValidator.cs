using Application.Core;
using FluentValidation;

namespace Application.Auth.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithError(AuthErrors.MissingEmail)
            .EmailAddress()
            .WithError(AuthErrors.InvalidEmail);
        
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.");
        
        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.");

        const int minPasswordLength = 8;
        
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithMessage($"Password must be at least {minPasswordLength} characters long.");
    }
}
