using FluentValidation;

namespace Application.ServiceRecords.GetById;

internal sealed class GetServiceRecordByIdQueryValidator : AbstractValidator<GetServiceRecordByIdQuery>
{
    public GetServiceRecordByIdQueryValidator()
    {
        RuleFor(q => q.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");

        RuleFor(q => q.ServiceRecordId)
            .NotEmpty()
            .WithMessage("ServiceRecordId is required.");
    }
}