using Application.Core;
using Domain.Enums;

namespace Application.EnergyEntries;

public static class EnergyEntryErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "EnergyEntries.Unauthorized",
        "You are not authorized to perform this action.");
    public static Error CreateFailed => Error.Failure(
        "FuelEntries.CreateFailed",
        "Failed to create fuel entry");

    public static Error NotFound(Guid entryId) => Error.NotFound(
        "EnergyEntries.NotFound",
        $"Energy entry with Id = '{entryId}' was not found.");

    public static Error DeleteFailed(Guid entryId) => Error.Failure(
        "EnergyEntries.DeleteFailed",
        $"Failed to delete energy entry with Id = '{entryId}'.");

    public static Error NotOwnedByUser(Guid entryId) => Error.Unauthorized(
        "EnergyEntries.NotOwnedByUser",
        $"Energy entry with Id = '{entryId}' does not belong to the current user.");

    public static Error IncompatiblePowerType(Guid vehicleId, PowerType powerType) => Error.Failure(
        "FuelEntries.IncompatiblePowerType",
        $"Vehicle with Id = '{vehicleId}' cannot be fueled because it has incompatible power type '{powerType}'.");

}