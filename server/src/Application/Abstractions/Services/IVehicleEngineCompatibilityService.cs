using Application.Core;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleEngineCompatibilityService
{
    Result ValidateEnergyTypeAssignment(Vehicle vehicle, EnergyType energyType);
    Result ValidateEngineCompatibility(EngineType engineType, IEnumerable<EnergyType> energyTypes);
    Result ValidateEngineCompatibility(EngineType engineType, EnergyType energyType);

    Task<bool> IsEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyType, CancellationToken cancellationToken = default);
    bool IsEnergyTypeCompatibleWithEngine(EnergyType energyType, EngineType engineType);
    Task<ICollection<EnergyType>> GetCompatibleEnergyTypesForEngine(EngineType engineType);
}