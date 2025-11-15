using Application.Abstractions;
using FluentValidation;

namespace Application.ServiceRecords.Update;

internal sealed class UpdateServiceRecordCommandValidator : AbstractValidator<UpdateServiceRecordCommand>
{
    public UpdateServiceRecordCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.ServiceRecordId)
            .NotEmpty()
            .WithMessage("ServiceRecordId is required.");

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");

        RuleFor(x => x.ServiceTypeId)
            .NotEmpty()
            .WithMessage("ServiceTypeId is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(64)
            .WithMessage("Title cannot exceed 64 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Mileage.HasValue)
            .WithMessage("Mileage cannot be negative.");

        RuleFor(x => x.ServiceDate)
            .NotEmpty()
            .LessThanOrEqualTo(dateTimeProvider.UtcNow)
            .WithMessage("Service date cannot be in the future.");

        RuleFor(x => x.ManualCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ManualCost.HasValue)
            .WithMessage("Manual cost cannot be negative.");
    }
}