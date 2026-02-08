using Application.Core;

namespace Application.ServiceRecords;

public static class ServiceRecordErrors
{
    public static readonly Error Forbidden = Error.Forbidden(
        "ServiceRecord.Forbidden",
        "You are not authorized to perform this action.");
    
    public static readonly Error NotFound = Error.NotFound(
        "ServiceRecord.NotFound",
        "Service record was not found.");

    public static Error GetTypesFailed => Error.Failure(
        "ServiceRecord.GetTypesFailed",
        "Failed to retrieve service types.");
    
    public static Error ServiceTypeNotFound(Guid serviceTypeTypeId) => Error.NotFound(
        "ServiceRecord.ServiceTypeNotFound",
        $"Service Type with Id = '{serviceTypeTypeId}' was not found.");
}