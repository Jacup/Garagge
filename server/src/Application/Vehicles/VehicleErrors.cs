using Application.Core;
using Domain.Enums;

namespace Application.Vehicles;

public static class VehicleErrors
{
    public static readonly Error Forbidden = Error.Forbidden(
        "Vehicle.Forbidden",
        "You are not authorized to perform this action.");

    public static readonly Error NotFound = Error.NotFound(
        "Vehicle.NotFound",
        "Vehicle was not found.");

    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Vehicle.ConcurrencyConflict",
        "Vehicle was modified by another process. Please try again.");

    public static Error CannotRemoveEnergyTypes(IReadOnlyList<EnergyType> energyTypes, int affectedEntryCount)
    {
        var energyTypesStr = string.Join(", ", energyTypes);

        return Error.Conflict(
            "Vehicle.CannotRemoveEnergyTypes",
            $"Cannot remove energy types [{energyTypesStr}] " +
            $"because {affectedEntryCount} entries are using them. Delete those entries first.");
    }
}