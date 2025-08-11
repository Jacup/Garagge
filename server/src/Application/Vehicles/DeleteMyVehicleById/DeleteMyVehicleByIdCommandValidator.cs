using FluentValidation;

namespace Application.Vehicles.DeleteMyVehicleById;

internal sealed class DeleteMyVehicleByIdCommandValidator : AbstractValidator<DeleteMyVehicleByIdCommand>
{
    public DeleteMyVehicleByIdCommandValidator()
    {
        RuleFor(c => c.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}

