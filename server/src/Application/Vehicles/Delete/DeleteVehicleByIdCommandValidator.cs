using FluentValidation;

namespace Application.Vehicles.Delete;

internal sealed class DeleteVehicleByIdCommandValidator : AbstractValidator<DeleteVehicleByIdCommand>
{
    public DeleteVehicleByIdCommandValidator()
    {
        RuleFor(c => c.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}

