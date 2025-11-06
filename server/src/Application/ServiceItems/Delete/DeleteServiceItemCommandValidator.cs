using FluentValidation;

namespace Application.ServiceItems.Delete;

internal sealed class DeleteServiceItemCommandValidator : AbstractValidator<DeleteServiceItemCommand>
{
    public DeleteServiceItemCommandValidator()
    {
        RuleFor(sr => sr.ServiceItemId)
            .NotEmpty()
            .WithMessage("ServiceItemId is required.");

        RuleFor(sr => sr.ServiceRecordId)
            .NotEmpty()
            .WithMessage("ServiceRecordId is required.");

        RuleFor(sr => sr.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}