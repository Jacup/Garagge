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

    public static Error CreateFailed => Error.Failure(
        "ServiceRecords.CreateFailed",
        "Failed to create service record.");

    public static Error DeleteFailed(Guid serviceRecordId) => Error.Failure(
        "ServiceRecords.DeleteFailed",
        "Failed to delete service record.");

    public static Error UpdateFailed(Guid serviceRecordId) => Error.Failure(
        "ServiceRecords.UpdateFailed",
        $"Failed to update service record with Id = '{serviceRecordId}'.");

    public static Error ServiceTypeNotFound(Guid serviceTypeTypeId) => Error.NotFound(
        "ServiceRecords.ServiceTypeNotFound",
        $"Service Type with Id = '{serviceTypeTypeId}' was not found.");
}