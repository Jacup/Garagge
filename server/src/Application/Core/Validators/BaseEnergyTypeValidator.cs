using Application.Abstractions.Services;
using Domain.Enums;
using FluentValidation;

namespace Application.Core.Validators;

public abstract class BaseEnergyTypeValidator<T>(IVehicleEngineCompatibilityService compatibilityService) : AbstractValidator<T>
{
    protected bool AreValidEnumValues(IEnumerable<EnergyType>? energyTypes)
    {
        return energyTypes?.All(Enum.IsDefined) ?? true;
    }

    protected bool AreUnique(IEnumerable<EnergyType>? energyTypes)
    {
        if (energyTypes == null) 
            return true;
        
        var list = energyTypes.ToList();
        return list.Distinct().Count() == list.Count;
    }

    protected bool AreAllCompatibleWithEngine(IEnumerable<EnergyType> energyTypes, EngineType engineType)
    {
        return energyTypes.All(et => compatibilityService.IsEnergyTypeCompatibleWithEngine(et, engineType));
    }

    protected string BuildIncompatibilityMessage(IEnumerable<EnergyType>? energyTypes, EngineType engineType)
    {
        var incompatibleTypes = energyTypes?.Where(et => !compatibilityService.IsEnergyTypeCompatibleWithEngine(et, engineType));
        
        var compatibleTypes = compatibilityService.GetCompatibleEnergyTypesForEngine(engineType);
        
        return $"Energy types [{string.Join(", ", incompatibleTypes ?? [])}] are not compatible with engine type '{engineType}'. " +
               $"Compatible energy types for {engineType} engine are: {string.Join(", ", compatibleTypes)}.";
    }
}
