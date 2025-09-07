using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

internal sealed class VehicleEngineCompatibilityService(IApplicationDbContext dbContext) : IVehicleEngineCompatibilityService
{
    public async Task<bool> IsEnergyTypeCompatibleAsync(Guid vehicleId, EnergyType energyType, CancellationToken cancellationToken = default)
    {
        var compatibleTypes = await GetCompatibleEnergyTypesAsync(vehicleId, cancellationToken);
        
        return compatibleTypes.Contains(energyType);
    }

    internal async Task<IEnumerable<EnergyType>> GetCompatibleEnergyTypesAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VehicleEnergyTypes
            .AsNoTracking()
            .Where(vet => vet.VehicleId == vehicleId)
            .Select(vet => vet.EnergyType)
            .ToListAsync(cancellationToken);
    }
}
