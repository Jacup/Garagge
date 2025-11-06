using FluentValidation;

namespace Application.ServiceItems.Delete;

public class DeleteServiceItemCommandValidator : AbstractValidator<DeleteServiceItemCommand>
{
    public DeleteServiceItemCommandValidator()
    {
        RuleFor(sr => sr.ServiceItemId)
            .NotEmpty()
            .WithMessage("ServiceItemId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("ServiceItemId must be a valid GUID.");
        
        RuleFor(sr => sr.ServiceRecordId)
            .NotEmpty()
            .WithMessage("ServiceRecordId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("ServiceRecordId must be a valid GUID.");
        
        RuleFor(sr => sr.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("VehicleId must be a valid GUID.");
    }
}