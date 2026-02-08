using Application.Core;
using Application.Users;
using FluentValidation;

namespace Application.Auth.Login;

internal class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithError(UserErrors.EmailRequired)
            .EmailAddress()
            .WithError(UserErrors.EmailInvalid);

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithError(UserErrors.PasswordRequired);
    }
}