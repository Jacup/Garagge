using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceItems.Delete;

internal sealed class DeleteServiceItemCommandValidator : AbstractValidator<DeleteServiceItemCommand>
{
    public DeleteServiceItemCommandValidator()
    {
        RuleFor(sr => sr.ServiceItemId)
            .NotEmpty()
            .WithError(ServiceItemsErrors.IdRequired);

        RuleFor(sr => sr.ServiceRecordId)
            .NotEmpty()
            .WithError(ServiceItemsErrors.ServiceRecordIdRequired);

        RuleFor(sr => sr.VehicleId)
            .NotEmpty()
            .WithError(ServiceItemsErrors.VehicleIdRequired);
    }
}