using FluentValidation;

namespace Application.Features.Vehicles.GetById;

internal sealed class GetVehicleByIdQueryValidator : AbstractValidator<GetVehicleByIdQuery>
{
    public GetVehicleByIdQueryValidator()
    {
        RuleFor(q => q.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}

