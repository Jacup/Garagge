using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceRecords.Delete;

internal sealed class DeleteServiceRecordCommandValidator : AbstractValidator<DeleteServiceRecordCommand>
{
    public DeleteServiceRecordCommandValidator()
    {
        RuleFor(sr => sr.ServiceRecordId)
            .NotEmpty()
            .WithError(ServiceRecordErrors.ServiceRecordIdRequired);

        RuleFor(sr => sr.VehicleId)
            .NotEmpty()
            .WithError(ServiceRecordErrors.VehicleIdRequired);
    }
}
