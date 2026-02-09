using Application.Abstractions;
using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceRecords.Create;

internal sealed class CreateServiceRecordsCommandValidator : AbstractValidator<CreateServiceRecordCommand>
{
    public CreateServiceRecordsCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        const int notesMaximumLength = 500;
        const int titleMaximumLength = 64;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithError(ServiceRecordErrors.TitleRequired)
            .MaximumLength(titleMaximumLength)
            .WithError(ServiceRecordErrors.TitleTooLong(titleMaximumLength));

        RuleFor(x => x.Notes)
            .MaximumLength(notesMaximumLength)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithError(ServiceRecordErrors.NotesTooLong(notesMaximumLength));

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
