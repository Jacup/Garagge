using FluentValidation;

namespace Application.VehicleEnergyTypes.Delete;

internal sealed class DeleteVehicleEnergyTypeCommandValidator : AbstractValidator<DeleteVehicleEnergyTypeCommand>
{
    public DeleteVehicleEnergyTypeCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage("ID is required and must be a valid GUID.");
        
        RuleFor(c => c.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required and must be a valid GUID.");
    }
}