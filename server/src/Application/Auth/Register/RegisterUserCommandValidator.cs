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
            .WithError(AuthErrors.MissingFirstName);
        
        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithError(AuthErrors.MissingLastName);

        const int minPasswordLength = 8;
        
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithError(AuthErrors.InvalidPassword(minPasswordLength));
    }
}
