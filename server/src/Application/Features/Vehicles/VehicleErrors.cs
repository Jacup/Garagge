using Application.Core;
using Domain.Enums;

namespace Application.Features.Vehicles;

public static class VehicleErrors
{
    public static readonly Error BrandRequired = Error.Validation(
        "Vehicle.BrandRequired",
        "Brand is required.");

    public static Error BrandTooLong(int characters) => Error.Validation(
        "Vehicle.BrandTooLong",
        $"Brand cannot exceed {characters} characters.");

    public static readonly Error ModelRequired = Error.Validation(
        "Vehicle.ModelRequired",
        "Model is required.");

    public static Error ModelTooLong(int characters) => Error.Validation(
        "Vehicle.ModelTooLong",
        $"Model cannot exceed {characters} characters.");

    public static readonly Error EngineTypeInvalid = Error.Validation(
        "Vehicle.EngineTypeInvalid",
        "Engine type is invalid.");

    public static Error ManufacturedYearInvalid(int minYear, int maxYear) => Error.Validation(
        "Vehicle.ManufacturedYearInvalid",
        $"Manufactured year must be between {minYear} and {maxYear}.");

    public static readonly Error TypeInvalid = Error.Validation(
        "Vehicle.TypeInvalid",
        $"Vehicle type must be valid. Valid values are: {string.Join(", ", Enum.GetNames<VehicleType>())}.");

    public static readonly Error VinInvalidLength = Error.Validation(
        "Vehicle.VinInvalidLength",
        "VIN must be exactly 17 characters long.");

    public static readonly Error EnergyTypesInvalid = Error.Validation(
        "Vehicle.EnergyTypesInvalid",
        $"All energy types must be valid. Valid values are: {string.Join(", ", Enum.GetNames<EnergyType>())}.");

    public static readonly Error EnergyTypesNotUnique = Error.Validation(
        "Vehicle.EnergyTypesNotUnique",
        "Energy types must be unique.");

    public static Error EnergyTypesIncompatible(string incompatibilityMessage) => Error.Validation(
        "Vehicle.EnergyTypesIncompatible",
        incompatibilityMessage);

    public static readonly Error NotFound = Error.NotFound(
        "Vehicle.NotFound",
        "Vehicle was not found.");

    public static readonly Error Forbidden = Error.Forbidden(
        "Vehicle.Forbidden",
        "You are not authorized to perform this action.");

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