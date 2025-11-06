using FluentValidation;

namespace Application.ServiceRecords.Delete;

public class DeleteServiceRecordCommandValidator : AbstractValidator<DeleteServiceRecordCommand>
{
    public DeleteServiceRecordCommandValidator()
    {
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