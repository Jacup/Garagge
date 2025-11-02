using Application.Core;

namespace Application.ServiceRecords;

public static class ServiceRecordErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "ServiceRecords.Unauthorized",
        "You are not authorized to perform this action.");
    
    public static Error NotFound(Guid recordId) => Error.NotFound(
        "ServiceRecords.NotFound",
        $"Service record with Id = '{recordId}' was not found.");
}