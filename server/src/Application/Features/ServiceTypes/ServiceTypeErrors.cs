using Application.Core;

namespace Application.Features.ServiceTypes;

public static class ServiceTypeErrors
{
    public static readonly Error IdRequired = Error.Validation(
        "ServiceType.IdRequired",
        "ID is required.");    
    
    public static readonly Error NameRequired = Error.Validation(
        "ServiceType.NameRequired",
        "Name is required.");

    public static Error NameTooLong(int maxLength) => Error.Validation(
        "ServiceType.NameTooLong",
        $"Name cannot exceed {maxLength} characters.");

    public static readonly Error ServiceRecordsExists = Error.Conflict(
        "ServiceType.ServiceRecordsExists",
        "Cannot delete service type because there are service records assigned.");

    public static readonly Error NotFound = Error.NotFound(
        "ServiceType.NotFound",
        "Service type not found.");
}
