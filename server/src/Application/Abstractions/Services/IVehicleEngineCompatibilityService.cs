using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleEngineCompatibilityService
{
    Task<bool> IsEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyType, CancellationToken cancellationToken = default);
}
