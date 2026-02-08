using Application.Core;

namespace Application.EnergyEntries;

public static class EnergyEntryErrors
{
    public static readonly Error Forbidden = Error.Forbidden(
        "EnergyEntry.Forbidden",
        "You are not authorized to perform this action.");

    public static readonly Error NotFound = Error.NotFound(
        "EnergyEntry.NotFound",
        "Energy entry was not found.");

    public static readonly Error MileageIncorrect = Error.Problem(
        "EnergyEntry.MileageIncorrect",
        "Mileage cannot be less than previous entry or greater than next entry.");

    public static readonly Error TypeIncompatible = Error.Problem(
        "EnergyEntry.TypeIncompatible",
        "This energy type is not supported by the vehicle.");
}