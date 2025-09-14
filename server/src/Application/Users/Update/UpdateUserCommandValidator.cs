using Application.Core;
using FluentValidation;

namespace Application.Users.Update;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
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