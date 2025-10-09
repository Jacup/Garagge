using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Core;
using Application.VehicleEnergyTypes;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

internal sealed class VehicleEngineCompatibilityService(IApplicationDbContext dbContext) : IVehicleEngineCompatibilityService
{
    public Result ValidateEnergyTypeAssignment(Vehicle vehicle, EnergyType energyType)
    {
        List<Result> validationResults =
        [
            ValidateDuplicateEnergyType(vehicle, energyType),
            ValidateEngineCompatibility(vehicle.EngineType, energyType)
        ];

        return Result.Combine(validationResults);
    }

    public Result ValidateEngineCompatibility(EngineType engineType, IEnumerable<EnergyType> energyTypes)
    {
        List<Result> validationResults = [];
        
        foreach (var energyType in energyTypes)
        {
            validationResults.Add(ValidateEngineCompatibility(engineType, energyType));
        }
        
        return Result.Combine(validationResults);
    }

    public Result ValidateEngineCompatibility(EngineType engineType, EnergyType requestEnergyType)
    {
        if (!IsEnergyTypeCompatibleWithEngine(requestEnergyType, engineType))
        {
            return Result.Failure(VehicleEnergyTypeErrors.IncompatibleWithEngine(requestEnergyType, engineType));
        }

        return Result.Success();
    }

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
            EngineType.Fuel or EngineType.Hybrid => IceFuels.Contains(energyType),
            EngineType.PlugInHybrid => PlugInHybridFuels.Contains(energyType),
            EngineType.Electric => energyType == EnergyType.Electric,
            EngineType.Hydrogen => energyType == EnergyType.Hydrogen,
            _ => false
        };
    }

    public Task<ICollection<EnergyType>> GetValidEnergyTypesForEngine(EngineType engineType)
    {
        ICollection<EnergyType> validTypes = engineType switch
        {
            EngineType.Fuel or EngineType.Hybrid => IceFuels,
            EngineType.PlugInHybrid => PlugInHybridFuels,
            EngineType.Electric => [ EnergyType.Electric ],
            EngineType.Hydrogen => [ EnergyType.Hydrogen ],
            _ => []
        };

        return Task.FromResult(validTypes);
    }

    internal static Result ValidateDuplicateEnergyType(Vehicle vehicle, EnergyType requestEnergyType)
    {
        if (vehicle.AllowedEnergyTypes.Any(e => e == requestEnergyType))
        {
            return Result.Failure(VehicleEnergyTypeErrors.AlreadyExists(vehicle.Id, requestEnergyType));
        }

        return Result.Success();
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