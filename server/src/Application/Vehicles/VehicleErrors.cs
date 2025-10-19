using Application.Core;
using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace Application.Vehicles;

public static class VehicleErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "Vehicles.Unauthorized",
        "You are not authorized to perform this action.");

    public static Error CreateFailed => Error.Failure(
        "Vehicles.CreateFailed",
        "Failed to create vehicle");

    public static Error UpdateFailed(Guid vehicleId) => Error.Failure(
        "Vehicles.UpdateFailed",
        $"Failed to update vehicle with Id = '{vehicleId}'");

    public static Error CannotRemoveEnergyTypesWithExistingEntries(Guid vehicleId, IReadOnlyList<EnergyType> energyTypes, int affectedEntryCount)
    {
        var energyTypesStr = string.Join(", ", energyTypes);

        return Error.Conflict(
            "Vehicles.CannotRemoveEnergyTypes",
            $"Cannot remove energy types [{energyTypesStr}] from vehicle {vehicleId} " +
            $"because there are {affectedEntryCount} energy entries using these types. " +
            $"Please delete those entries first.");
    }

    public static Error DeleteFailed(Guid vehicleId) => Error.Failure(
        "Vehicles.DeleteFailed",
        $"Failed to delete vehicle with Id = '{vehicleId}'");

    public static Error NotFoundForUser(Guid userId) => Error.NotFound(
        "Vehicles.NotFound",
        $"Not found any vehicles with the UserId = '{userId}'");

    public static Error NotFound(Guid vehicleId) => Error.NotFound(
        "Vehicles.NotFound",
        $"Not found vehicle with Id = '{vehicleId}'");

    public static Error ConcurrencyConflict(Guid requestVehicleId) => Error.Conflict(
        "Vehicles.ConcurrencyConflict",
        $"The vehicle with Id = '{requestVehicleId}' was modified by another process. Please try again.");
}