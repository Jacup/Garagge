using FluentValidation;

namespace Application.Vehicles.GetMyVehicleById;

internal sealed class GetMyVehicleByIdQueryValidator : AbstractValidator<GetMyVehicleByIdQuery>
{
    public GetMyVehicleByIdQueryValidator()
    {
        RuleFor(q => q.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
        
        RuleFor(q => q.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}

