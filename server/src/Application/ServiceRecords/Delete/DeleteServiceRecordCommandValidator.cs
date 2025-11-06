using FluentValidation;

namespace Application.ServiceRecords.Delete;

internal sealed class DeleteServiceRecordCommandValidator : AbstractValidator<DeleteServiceRecordCommand>
{
    public DeleteServiceRecordCommandValidator()
    {
        RuleFor(sr => sr.ServiceRecordId)
            .NotEmpty()
            .WithMessage("ServiceRecordId is required.");

        RuleFor(sr => sr.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}