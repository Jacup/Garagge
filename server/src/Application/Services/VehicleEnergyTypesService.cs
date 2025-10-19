using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class VehicleEnergyTypesService(IApplicationDbContext dbContext) : IVehicleEnergyTypesService
{
    public async Task<Result<VehicleEnergyTypesUpdateResult>> PrepareUpdateAsync(
        Vehicle vehicle,
        IEnumerable<EnergyType> requestedEnergyTypes,
        CancellationToken cancellationToken)
    {
        var requestedTypesList = requestedEnergyTypes.ToList();

        if (requestedTypesList.Count == 0)
            return Result.Success(VehicleEnergyTypesUpdateResult.NoChanges());

        var currentTypes = vehicle.AllowedEnergyTypes.ToList();
        var typesToAdd = requestedTypesList.Except(currentTypes).ToList();
        var typesToRemove = currentTypes.Except(requestedTypesList).ToList();

        if (typesToRemove.Count == 0)
        {
            return Result.Success(new VehicleEnergyTypesUpdateResult(
                TypesToAdd: typesToAdd,
                TypesToRemove: [],
                ConflictingEntries: []));
        }

        var conflictingEntries = await dbContext.EnergyEntries
            .Where(ee => ee.VehicleId == vehicle.Id && typesToRemove.Contains(ee.Type))
            .ToListAsync(cancellationToken);

        if (conflictingEntries.Count > 0)
        {
            var conflictingTypes = conflictingEntries
                .Select(ee => ee.Type)
                .Distinct()
                .ToList();

            return Result.Failure<VehicleEnergyTypesUpdateResult>(
                VehicleErrors.UpdateFailedEnergyEntryExists(
                    vehicle.Id,
                    conflictingTypes,
                    conflictingEntries.Count));
        }

        return Result.Success(new VehicleEnergyTypesUpdateResult(
            TypesToAdd: typesToAdd,
            TypesToRemove: typesToRemove,
            ConflictingEntries: []));
    }
}