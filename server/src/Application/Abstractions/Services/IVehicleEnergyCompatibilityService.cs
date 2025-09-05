using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleEnergyCompatibilityService
{
    Task<bool> IsEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyType, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnergyType>> GetCompatibleEnergyTypesAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<bool> IsAnyEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyTypes, CancellationToken cancellationToken = default);
}
