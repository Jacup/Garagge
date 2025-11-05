using Application.Core;

namespace Application.ServiceItems;

public static class ServiceItemsErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "ServiceItems.Unauthorized",
        "You are not authorized to perform this action.");
    
    public static Error NotFound(Guid recordId) => Error.NotFound(
        "ServiceItems.NotFound",
        $"Service Item with Id = '{recordId}' was not found.");

    public static Error CreateFailed => Error.Failure(
        "ServiceItems.CreateFailed",
        "Failed to create service item.");
}