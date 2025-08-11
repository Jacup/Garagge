using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.");
        
        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.");
        
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        const int minPasswordLength = 8;
        
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithMessage($"Password must be at least {minPasswordLength} characters long.");
    }
}
