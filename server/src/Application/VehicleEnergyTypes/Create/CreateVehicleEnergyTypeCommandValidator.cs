using Domain.Enums;
using FluentValidation;

namespace Application.VehicleEnergyTypes.Create;

internal sealed class CreateVehicleEnergyTypeCommandValidator : AbstractValidator<CreateVehicleEnergyTypeCommand>
{
    public CreateVehicleEnergyTypeCommandValidator()
    {
        RuleFor(c => c.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required and must be a valid GUID.");
        
        RuleFor(c => c.EnergyType)
            .IsInEnum()
            .WithMessage($"Energy Type must be a valid enum value. Valid values are: {string.Join(", ", Enum.GetNames<EnergyType>())}.");
    }
}