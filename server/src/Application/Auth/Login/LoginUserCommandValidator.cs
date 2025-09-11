using Application.Auth.Register;
using Application.Core;
using FluentValidation;

namespace Application.Auth.Login;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithError(AuthErrors.MissingEmail)
            .EmailAddress()
            .WithError(AuthErrors.InvalidEmail);

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithError(AuthErrors.MissingPassword);
    }
}