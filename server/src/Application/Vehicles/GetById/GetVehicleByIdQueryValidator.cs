using FluentValidation;

namespace Application.Vehicles.GetById;

internal sealed class GetVehicleByIdQueryValidator : AbstractValidator<GetVehicleByIdQuery>
{
    public GetVehicleByIdQueryValidator()
    {
        RuleFor(q => q.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}

