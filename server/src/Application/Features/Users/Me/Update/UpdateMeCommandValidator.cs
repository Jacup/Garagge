using Application.Core;
using FluentValidation;

namespace Application.Features.Users.Me.Update;

public class UpdateMeCommandValidator : AbstractValidator<UpdateMeCommand>
{
    public UpdateMeCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithError(UserErrors.EmailRequired)
            .EmailAddress()
            .WithError(UserErrors.EmailInvalid);

        const int maximumLength = 64;
        
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .WithError(UserErrors.FirstNameRequired)
            .MaximumLength(maximumLength)
            .WithError(UserErrors.FirstNameTooLong(maximumLength));
        
        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithError(UserErrors.LastNameRequired)
            .MaximumLength(maximumLength)
            .WithError(UserErrors.LastNameTooLong(maximumLength));
    }
}