using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.Update;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class VehicleUpdateValidationService(IApplicationDbContext dbContext) : IVehicleUpdateValidationService
{
    public async Task<Result<VehicleEnergyTypesUpdatePlan>> ValidateEnergyTypesChangeAsync(
        Vehicle vehicle, 
        IEnumerable<EnergyType> requestedEnergyTypes,
        CancellationToken cancellationToken)
    {
        var requestedTypesList = requestedEnergyTypes.ToList();

        var currentTypes = vehicle.AllowedEnergyTypes.ToList();
        var typesToAdd = requestedTypesList.Except(currentTypes).ToList();
        var typesToRemove = currentTypes.Except(requestedTypesList).ToList();

        if (typesToAdd.Count == 0 && typesToRemove.Count == 0)
        {
            return Result.Success(VehicleEnergyTypesUpdatePlan.NoChanges());
        }

        if (typesToRemove.Count == 0)
        {
            return Result.Success(new VehicleEnergyTypesUpdatePlan(
                TypesToAdd: typesToAdd,
                TypesToRemove: [],
                ConflictingEnergyTypes: []));
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

            return Result.Failure<VehicleEnergyTypesUpdatePlan>(
                VehicleErrors.CannotRemoveEnergyTypes(conflictingTypes, conflictingEntries.Count));
        }

        return Result.Success(new VehicleEnergyTypesUpdatePlan(
            TypesToAdd: typesToAdd,
            TypesToRemove: typesToRemove,
            ConflictingEnergyTypes: []));
    }
}