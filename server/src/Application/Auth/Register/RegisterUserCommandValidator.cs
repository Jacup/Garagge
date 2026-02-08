using Application.Core;
using Application.Users;
using FluentValidation;

namespace Application.Auth.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithError(UserErrors.EmailRequired)
            .EmailAddress()
            .WithError(UserErrors.EmailInvalid);

        RuleFor(c => c.FirstName)
            .NotEmpty()
            .WithError(UserErrors.FirstNameRequired);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithError(UserErrors.LastNameRequired);

        const int minPasswordLength = 8;

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithError(UserErrors.PasswordTooShort(minPasswordLength));
    }
}