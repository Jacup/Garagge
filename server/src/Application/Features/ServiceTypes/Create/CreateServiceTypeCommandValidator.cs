using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceTypes.Create;

internal sealed class CreateServiceTypeCommandValidator : AbstractValidator<CreateServiceTypeCommand>
{
    public CreateServiceTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithError(ServiceTypeErrors.NameRequired)
            .MaximumLength(64)
            .WithError(ServiceTypeErrors.NameTooLong(64));
    }
}
