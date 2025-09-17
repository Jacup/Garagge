using Application.Abstractions;
using Application.Abstractions.Services;
using Domain.Enums;
using FluentValidation;

namespace Application.Vehicles.Create;

internal sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    private readonly IVehicleEngineCompatibilityService _compatibilityService;

    public CreateVehicleCommandValidator(
        IDateTimeProvider dateTimeProvider,
        IVehicleEngineCompatibilityService compatibilityService)
    {
        _compatibilityService = compatibilityService;

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
            .When(c => c.EnergyTypes is not null)
            .WithMessage($"All energy types must be valid enum values. Valid values are: {string.Join(", ", Enum.GetNames<EnergyType>())}.")
            .Must(DoesNotExceedMaximumCount)
            .When(c => c.EnergyTypes is not null)
            .WithMessage("Cannot specify more energy types than available in the system.")
            .Must(AreUnique)
            .When(c => c.EnergyTypes is not null)
            .WithMessage("Energy types must be unique - no duplicates allowed.")
            .Must(AreCompatibleWithEngine)
            .When(c => c.EnergyTypes is not null)
            .WithMessage(BuildIncompatibilityMessage);
    }

    private static bool AreValidEnumValues(IEnumerable<EnergyType>? energyTypes)
    {
        return energyTypes?.All(Enum.IsDefined) ?? true;
    }

    private static bool DoesNotExceedMaximumCount(IEnumerable<EnergyType>? energyTypes)
    {
        return energyTypes?.Count() <= 8;
    }

    private static bool AreUnique(IEnumerable<EnergyType>? energyTypes)
    {
        if (energyTypes == null) 
            return true;
        
        var list = energyTypes.ToList();
        return list.Distinct().Count() == list.Count;
    }

    private bool AreCompatibleWithEngine(CreateVehicleCommand command, IEnumerable<EnergyType>? energyTypes)
    {
        return energyTypes?.All(et => _compatibilityService.IsEnergyTypeCompatibleWithEngine(et, command.EngineType)) ?? true;
    }

    private string BuildIncompatibilityMessage(CreateVehicleCommand command, IEnumerable<EnergyType>? energyTypes)
    {
        var incompatibleTypes = energyTypes?.Where(et => 
            !_compatibilityService.IsEnergyTypeCompatibleWithEngine(et, command.EngineType));
        
        var compatibleTypes = GetCompatibleEnergyTypes(command.EngineType);
        
        return $"Energy types [{string.Join(", ", incompatibleTypes ?? [])}] are not compatible with engine type '{command.EngineType}'. " +
               $"Compatible energy types for {command.EngineType} engine are: {string.Join(", ", compatibleTypes)}.";
    }

    private IEnumerable<EnergyType> GetCompatibleEnergyTypes(EngineType engineType)
    {
        return Enum.GetValues<EnergyType>()
            .Where(et => _compatibilityService.IsEnergyTypeCompatibleWithEngine(et, engineType));
    }
}