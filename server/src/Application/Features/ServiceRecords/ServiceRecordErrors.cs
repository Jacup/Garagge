﻿using Application.Core;

namespace Application.Features.ServiceRecords;

public static class ServiceRecordErrors
{
    public static readonly Error ServiceRecordIdRequired = Error.Validation(
        "ServiceRecord.ServiceRecordIdRequired",
        "Service record ID is required.");

    public static readonly Error VehicleIdRequired = Error.Validation(
        "ServiceRecord.VehicleIdRequired",
        "Vehicle ID is required.");

    public static readonly Error TypeRequired = Error.Validation(
        "ServiceRecord.TypeRequired",
        "Type is required.");

    public static readonly Error TitleRequired = Error.Validation(
        "ServiceRecord.TitleRequired",
        "Title is required.");

    public static Error TitleTooLong(int maxLength) => Error.Validation(
        "ServiceRecord.TitleTooLong",
        $"Title cannot exceed {maxLength} characters.");

    public static Error NotesTooLong(int maxLength) => Error.Validation(
        "ServiceRecord.NotesTooLong",
        $"Notes cannot exceed {maxLength} characters.");

    public static readonly Error TypeInvalid = Error.Validation(
        "ServiceRecord.TypeInvalid",
        "Type is not a valid enum value.");
    
    public static readonly Error MileageNegative = Error.Validation(
        "ServiceRecord.MileageNegative",
        "Mileage cannot be negative.");

    public static readonly Error ServiceDateRequired = Error.Validation(
        "ServiceRecord.ServiceDateRequired",
        "Service date is required.");

    public static readonly Error ServiceDateInFuture = Error.Validation(
        "ServiceRecord.ServiceDateInFuture",
        "Service date cannot be in the future.");

    public static readonly Error ManualCostNegative = Error.Validation(
        "ServiceRecord.ManualCostNegative",
        "Manual cost cannot be negative.");

    public static readonly Error NotFound = Error.NotFound(
        "ServiceRecord.NotFound",
        "Service record was not found");

    public static readonly Error PageInvalid = Error.Validation(
        "EnergyEntry.PageInvalid",
        "Page must be greater than 0.");

    public static readonly Error PageSizeInvalid = Error.Validation(
        "EnergyEntry.PageSizeInvalid",
        "Page size must be between 1 and 100.");

    public static readonly Error DateFilterInvalid = Error.Validation(
        "EnergyEntry.DateFilterInvalid",
        "DateFrom must be before or equal to DateTo.");

    public static readonly Error SortByInvalid = Error.Validation(
        "EnergyEntry.SortByInvalid",
        "Invalid sortBy value. Allowed values: servicedate, totalcost, mileage, title");
}
