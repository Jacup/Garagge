using Application.Abstractions;
using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceRecords.Update;

internal sealed class UpdateServiceRecordCommandValidator : AbstractValidator<UpdateServiceRecordCommand>
{
    public UpdateServiceRecordCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.ServiceRecordId)
            .NotEmpty()
            .WithError(ServiceRecordErrors.ServiceRecordIdRequired);

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithError(ServiceRecordErrors.VehicleIdRequired);

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithError(ServiceRecordErrors.TypeRequired)
            .IsInEnum()
            .WithError(ServiceRecordErrors.TypeRequired);

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithError(ServiceRecordErrors.TitleRequired)
            .MaximumLength(64)
            .WithError(ServiceRecordErrors.TitleTooLong(64));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithError(ServiceRecordErrors.NotesTooLong(500));

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Mileage.HasValue)
            .WithError(ServiceRecordErrors.MileageNegative);

        RuleFor(x => x.ServiceDate)
            .NotEmpty()
            .WithError(ServiceRecordErrors.ServiceDateRequired)
            .LessThanOrEqualTo(dateTimeProvider.UtcNow)
            .WithError(ServiceRecordErrors.ServiceDateInFuture);

        RuleFor(x => x.ManualCost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ManualCost.HasValue)
            .WithError(ServiceRecordErrors.ManualCostNegative);
    }
}
