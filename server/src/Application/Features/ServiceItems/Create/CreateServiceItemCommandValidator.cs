using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceItems.Create;

internal sealed class CreateServiceItemCommandValidator : AbstractValidator<CreateServiceItemCommand>
{
    public CreateServiceItemCommandValidator()
    {
        RuleFor(sr => sr.ServiceRecordId)
            .NotEmpty()
            .WithError(ServiceItemsErrors.ServiceRecordIdRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithError(ServiceItemsErrors.NameRequired)
            .MaximumLength(128)
            .WithError(ServiceItemsErrors.NameTooLong(128));

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithError(ServiceItemsErrors.InvalidType);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithError(ServiceItemsErrors.UnitPriceNegative);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithError(ServiceItemsErrors.QuantityInvalid);

        RuleFor(x => x.PartNumber)
            .MaximumLength(64)
            .WithError(ServiceItemsErrors.PartNumberTooLong(64))
            .When(x => !string.IsNullOrEmpty(x.PartNumber));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithError(ServiceItemsErrors.NotesTooLong(500))
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}