using Application.Core;
using Application.Users;
using FluentValidation;

namespace Application.Auth.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        const int minPasswordLength = 8;
        
        RuleFor(c => c.CurrentPassword)
            .NotEmpty()
            .WithError(UserErrors.PasswordRequired);
        
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithError(UserErrors.PasswordTooShort(minPasswordLength));
        
        RuleFor(c => c.NewPassword)
            .NotEqual(c => c.CurrentPassword)
            .WithError(UserErrors.PasswordSameAsOld);
    }
}