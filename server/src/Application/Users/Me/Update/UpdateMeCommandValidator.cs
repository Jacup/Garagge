using Application.Core;
using FluentValidation;

namespace Application.Users.Me.Update;

public class UpdateMeCommandValidator : AbstractValidator<UpdateMeCommand>
{
    public UpdateMeCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithError(UserErrors.MissingEmail)
            .EmailAddress()
            .WithError(UserErrors.InvalidEmail);
        
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .WithError(UserErrors.MissingFirstName);
        
        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithError(UserErrors.MissingLastName);
    }
}