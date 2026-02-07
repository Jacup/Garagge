using Application.Core;
using FluentValidation;

namespace Application.Users.Me.Update;

public class UpdateMeCommandValidator : AbstractValidator<UpdateMeCommand>
{
    public UpdateMeCommandValidator()
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
    }
}