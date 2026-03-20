using Application.Core;
using Domain.Enums;
using Domain.Enums.Energy;

namespace Application.Features.EnergyEntries;

public static class EnergyEntryErrors
{
    public static readonly Error IdRequired = Error.Validation(
        "EnergyEntry.IdRequired",
        "ID is required.");

    public static readonly Error VehicleIdRequired = Error.Validation(
        "EnergyEntry.VehicleIdRequired",
        "Vehicle ID is required.");

    public static readonly Error VehicleNotFound = Error.Validation(
        "EnergyEntry.VehicleNotFound",
        "Vehicle was not found.");
    
    public static readonly Error UserIdRequired = Error.Validation(
        "EnergyEntry.UserIdRequired",
        "User ID is required.");

    public static readonly Error DateRequired = Error.Validation(
        "EnergyEntry.DateRequired",
        "Date is required.");

    public static readonly Error DateInFuture = Error.Validation(
        "EnergyEntry.DateInFuture",
        "Date cannot be in the future.");

    public static readonly Error MileageRequired = Error.Validation(
        "EnergyEntry.MileageRequired",
        "Mileage is required.");

    public static readonly Error MileageInvalid = Error.Validation(
        "EnergyEntry.MileageInvalid",
        "Mileage must be greater than 0.");

    public static readonly Error TypeRequired = Error.Validation(
        "EnergyEntry.TypeRequired",
        "Energy type is required.");

    public static readonly Error TypeInvalid = Error.Validation(
        "EnergyEntry.TypeInvalid",
        $"Energy type is invalid. Valid values are: {string.Join(", ", Enum.GetNames<EnergyType>())}.");

    public static readonly Error PeriodInvalid = Error.Validation(
        "EnergyEntry.PeriodInvalid",
        $"Period is invalid. Valid values are: {string.Join(", ", Enum.GetNames<StatsPeriod>())}.");

    public static readonly Error EnergyUnitRequired = Error.Validation(
        "EnergyEntry.EnergyUnitRequired",
        "Energy unit is required.");

    public static readonly Error EnergyUnitInvalid = Error.Validation(
        "EnergyEntry.EnergyUnitInvalid",
        $"Energy unit is invalid. Valid values are: {string.Join(", ", Enum.GetNames<EnergyUnit>())}.");

    public static readonly Error VolumeInvalid = Error.Validation(
        "EnergyEntry.VolumeInvalid",
        "Volume must be greater than 0.");

    public static readonly Error CostInvalid = Error.Validation(
        "EnergyEntry.CostInvalid",
        "Cost must be greater than 0.");

    public static readonly Error PricePerUnitInvalid = Error.Validation(
        "EnergyEntry.PricePerUnitInvalid",
        "Price per unit must be greater than 0.");

    public static readonly Error MileageIncorrect = Error.Problem(
        "EnergyEntry.MileageIncorrect",
        "Mileage cannot be less than previous entry or greater than next entry.");

    public static readonly Error TypeIncompatible = Error.Problem(
        "EnergyEntry.TypeIncompatible",
        "This energy type is not supported by the vehicle.");

    public static readonly Error PageInvalid = Error.Validation(
        "EnergyEntry.PageInvalid",
        "Page must be greater than 0.");

    public static readonly Error PageSizeInvalid = Error.Validation(
        "EnergyEntry.PageSizeInvalid",
        "Page size must be between 1 and 100.");

    public static readonly Error Forbidden = Error.Forbidden(
        "EnergyEntry.Forbidden",
        "You are not authorized to perform this action.");

    public static readonly Error NotFound = Error.NotFound(
        "EnergyEntry.NotFound",
        "Energy entry was not found.");
}