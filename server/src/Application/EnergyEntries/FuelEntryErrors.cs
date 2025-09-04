using Application.Core;
using Domain.Enums;

namespace Application.EnergyEntries;

public static class FuelEntryErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "FuelEntries.Unauthorized",
        "You are not authorized to perform this action.");

    public static Error CreateFailed => Error.Failure(
        "FuelEntries.CreateFailed",
        "Failed to create fuel entry");

    public static Error EditFailed(Guid vehicleId) => Error.Failure(
        "FuelEntries.EditFailed",
        $"Failed to edit fuel entry with Id = '{vehicleId}'");

    public static Error DeleteFailed(Guid vehicleId) => Error.Failure(
        "FuelEntries.DeleteFailed",
        $"Failed to delete fuel entry with Id = '{vehicleId}'");

    public static Error NotFoundForUser(Guid userId) => Error.NotFound(
        "FuelEntries.NotFound",
        $"Not found any fuel entries with the UserId = '{userId}'");

    public static Error NotFound(Guid fuelEntryId) => Error.NotFound(
        "FuelEntries.NotFound",
        $"Not found fuel entry with Id = '{fuelEntryId}'");

    public static Error IncompatiblePowerType(Guid vehicleId, PowerType powerType) => Error.Failure(
        "FuelEntries.IncompatiblePowerType",
        $"Vehicle with Id = '{vehicleId}' cannot be fueled because it has incompatible power type '{powerType}'.");
}