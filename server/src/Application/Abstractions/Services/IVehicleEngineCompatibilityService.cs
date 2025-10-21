using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleEngineCompatibilityService
{
    Task<bool> IsEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyType, CancellationToken cancellationToken = default);
    bool IsEnergyTypeCompatibleWithEngine(EnergyType energyType, EngineType engineType);
    Task<ICollection<EnergyType>> GetCompatibleEnergyTypesForEngine(EngineType engineType);
}