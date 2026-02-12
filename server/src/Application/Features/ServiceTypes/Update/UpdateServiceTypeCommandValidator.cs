using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceTypes.Update;

internal sealed class UpdateServiceTypeCommandValidator : AbstractValidator<UpdateServiceTypeCommand>
{
    public UpdateServiceTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithError(ServiceTypeErrors.IdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithError(ServiceTypeErrors.NameRequired)
            .MaximumLength(64)
            .WithError(ServiceTypeErrors.NameTooLong(64));
    }
}
