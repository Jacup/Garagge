using FluentValidation;

namespace Application.Features.Vehicles.Delete;

internal sealed class DeleteVehicleByIdCommandValidator : AbstractValidator<DeleteVehicleByIdCommand>
{
    public DeleteVehicleByIdCommandValidator()
    {
        RuleFor(c => c.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required.");
    }
}

