using Application.Abstractions;
using Application.Abstractions.Services;
using Application.Core.Validators;
using Domain.Enums;
using FluentValidation;

namespace Application.Vehicles.Update;

internal sealed class UpdateVehicleCommandValidator : BaseEnergyTypeValidator<UpdateVehicleCommand>
{
    public UpdateVehicleCommandValidator(IDateTimeProvider dateTimeProvider, IVehicleEngineCompatibilityService compatibilityService)
        : base(compatibilityService)
    {
        RuleFor(c => c.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required and must be a valid GUID.");

        RuleFor(c => c.Brand)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("Brand is required and must not exceed 64 characters.");

        RuleFor(c => c.Model)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("Model is required and must not exceed 64 characters.");

        RuleFor(c => c.EngineType)
            .IsInEnum()
            .WithMessage($"Power type must be a valid enum value. Valid values are: {string.Join(", ", Enum.GetNames<EngineType>())}.");

        const int firstCarManufacturedYear = 1886;

        RuleFor(c => c.ManufacturedYear)
            .GreaterThanOrEqualTo(firstCarManufacturedYear)
            .LessThanOrEqualTo(dateTimeProvider.UtcNow.Year)
            .When(c => c.ManufacturedYear.HasValue)
            .WithMessage($"Manufactured year must be between {firstCarManufacturedYear} and the current year.");

        RuleFor(c => c.Type)
            .IsInEnum()
            .When(c => c.Type.HasValue)
            .WithMessage($"Vehicle type must be a valid enum value. Valid values are: {string.Join(", ", Enum.GetNames<VehicleType>())}.");

        RuleFor(c => c.VIN)
            .Length(17)
            .When(c => c.VIN is not null)
            .WithMessage("VIN must be exactly 17 characters long.");

        ConfigureEnergyTypesValidation();
    }

    private void ConfigureEnergyTypesValidation()
    {
        RuleFor(c => c.EnergyTypes)
            .Must(AreValidEnumValues)
            .WithMessage($"All energy types must be valid enum values. Valid values are: {string.Join(", ", Enum.GetNames<EnergyType>())}.")
            .Must(AreUnique)
            .WithMessage("Energy types must be unique")
            .Must((command, energyTypes) => energyTypes == null || AreAllCompatibleWithEngine(energyTypes, command.EngineType))
            .WithMessage((command, energyTypes) => BuildIncompatibilityMessage(energyTypes, command.EngineType));
    }
}