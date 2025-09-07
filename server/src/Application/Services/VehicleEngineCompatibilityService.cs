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

    public async Task<IEnumerable<EnergyType>> GetCompatibleEnergyTypesAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.VehicleEnergyTypes
            .AsNoTracking()
            .Where(vet => vet.VehicleId == vehicleId)
            .Select(vet => vet.EnergyType)
            .ToListAsync(cancellationToken);
    }

    public bool IsEnergyTypeCompatibleWithEngine(EnergyType energyType, EngineType engineType)
    {
        return engineType switch
        {
            EngineType.Fuel => IceFuels.Contains(energyType),
            EngineType.Hybrid => IceFuels.Contains(energyType),
            EngineType.PlugInHybrid => PlugInHybridFuels.Contains(energyType), 
            EngineType.Electric => energyType == EnergyType.Electric,
            EngineType.Hydrogen => energyType == EnergyType.Hydrogen,
            _ => false
        };
    }

    private static readonly EnergyType[] IceFuels =
    [
        EnergyType.Gasoline,
        EnergyType.Diesel,
        EnergyType.LPG,
        EnergyType.CNG,
        EnergyType.Ethanol,
        EnergyType.Biofuel
    ];

    private static readonly EnergyType[] PlugInHybridFuels =
    [
        ..IceFuels,
        EnergyType.Electric
    ];
}
