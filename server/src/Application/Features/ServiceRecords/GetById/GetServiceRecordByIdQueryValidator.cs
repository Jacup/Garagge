using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceRecords.GetById;

internal sealed class GetServiceRecordByIdQueryValidator : AbstractValidator<GetServiceRecordByIdQuery>
{
    public GetServiceRecordByIdQueryValidator()
    {
        RuleFor(q => q.VehicleId)
            .NotEmpty()
            .WithError(ServiceRecordErrors.VehicleIdRequired);

        RuleFor(q => q.ServiceRecordId)
            .NotEmpty()
            .WithError(ServiceRecordErrors.ServiceRecordIdRequired);
    }
}
