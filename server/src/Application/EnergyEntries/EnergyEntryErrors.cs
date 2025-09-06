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

    public static Error IncorrectMileage => Error.Failure(
        "EnergyEntries.IncorrectMileage",
        "The mileage of the energy entry cannot be less than the mileage of the last energy entry or greater than the mileage of the next energy entry.");
    
    public static Error NotFound(Guid entryId) => Error.NotFound(
        "EnergyEntries.NotFound",
        $"Energy entry with Id = '{entryId}' was not found.");

    public static Error Invalid => Error.NotFound(
        "EnergyEntries.Invalid",
        "VehicleId is invalid for the specified energy entry.");
    
    public static Error DeleteFailed(Guid entryId) => Error.Failure(
        "EnergyEntries.DeleteFailed",
        $"Failed to delete energy entry with Id = '{entryId}'.");

    public static Error NotOwnedByUser(Guid entryId) => Error.Unauthorized(
        "EnergyEntries.NotOwnedByUser",
        $"Energy entry with Id = '{entryId}' does not belong to the current user.");

    public static Error IncompatibleEnergyType(Guid vehicleId, EnergyType energyType) => Error.Failure(
        "FuelEntries.IncompatibleEnergyType",
        $"Cannot add '{energyType}' to vehicle with Id = '{vehicleId}' because vehicle does not support this type of energy.");
}