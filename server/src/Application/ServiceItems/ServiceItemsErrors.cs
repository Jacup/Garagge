using Application.Core;

namespace Application.ServiceItems;

public static class ServiceItemsErrors
{
    public static readonly Error Forbidden = Error.Forbidden(
        "ServiceItem.Forbidden",
        "You are not authorized to perform this action.");

    public static readonly Error NotFound = Error.NotFound(
        "ServiceItem.NotFound",
        "Service item was not found.");
}