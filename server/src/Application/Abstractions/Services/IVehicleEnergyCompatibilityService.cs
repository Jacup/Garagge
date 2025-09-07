using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleEnergyCompatibilityService
{
    Task<bool> IsEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyType, CancellationToken cancellationToken = default);
}
