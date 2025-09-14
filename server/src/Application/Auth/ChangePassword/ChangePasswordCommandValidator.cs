using Application.Core;
using FluentValidation;

namespace Application.Auth.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        const int minPasswordLength = 8;
        
        RuleFor(c => c.CurrentPassword)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithError(AuthErrors.InvalidPassword(minPasswordLength));
        
        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(minPasswordLength)
            .WithError(AuthErrors.InvalidPassword(minPasswordLength));
        
        RuleFor(c => c.NewPassword)
            .NotEqual(c => c.CurrentPassword)
            .WithError(AuthErrors.NewPasswordSameAsOld);
    }
}