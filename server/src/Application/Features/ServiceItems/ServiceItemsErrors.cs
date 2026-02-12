using Application.Core;

namespace Application.Features.ServiceItems;

public static class ServiceItemsErrors
{
    public static readonly Error IdRequired = Error.Validation(
        "ServiceItem.IdRequired", 
        "ID is required.");
    
    public static readonly Error ServiceRecordIdRequired = Error.Validation(
        "ServiceItem.ServiceRecordIdRequired", 
        "Service Record ID is required.");   
    
    public static readonly Error VehicleIdRequired = Error.Validation(
        "ServiceItem.VehicleIdRequired", 
        "Vehicle ID is required.");
    
    public static readonly Error NameRequired = Error.Validation(
        "ServiceItem.NameRequired",
        "Name is required.");

    public static Error NameTooLong(int length) => Error.Validation(
        "ServiceItem.NameTooLong",
        $"Name cannot exceed {length} characters.");

    public static readonly Error InvalidType = Error.Validation(
        "ServiceItem.InvalidType",
        "Type must be a valid ServiceItemType.");

    public static readonly Error UnitPriceNegative = Error.Validation(
        "ServiceItem.UnitPriceNegative",
        "Unit price cannot be negative.");

    public static readonly Error QuantityInvalid = Error.Validation(
        "ServiceItem.QuantityInvalid",
        "Quantity must be greater than zero.");

    public static Error PartNumberTooLong(int length) => Error.Validation(
        "ServiceItem.PartNumberTooLong",
        $"Part number cannot exceed {length} characters.");

    public static Error NotesTooLong(int length) => Error.Validation(
        "ServiceItem.NotesTooLong",
        $"Notes cannot exceed {length} characters.");
    
    public static readonly Error NotFound = Error.NotFound(
        "ServiceItem.NotFound",
        "Service item was not found.");
}