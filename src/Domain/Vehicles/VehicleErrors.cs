using SharedKernel;

namespace Domain.Vehicles;

public static class VehicleErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "Vehicles.Unauthorized",
        "You are not authorized to perform this action.");
    
    public static Error NotFoundForUser(Guid userId) => Error.NotFound(
        "Vehicles.NotFound",
        $"Not found any cars with the UserId = '{userId}'");
}