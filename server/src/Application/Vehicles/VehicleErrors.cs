using Application.Core;

namespace Application.Vehicles;

public static class VehicleErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "Vehicles.Unauthorized",
        "You are not authorized to perform this action.");
    
    public static Error CreateFailed => Error.Failure(
        "Vehicles.CreateFailed",
        "Failed to create vehicle");    
    
    public static Error UpdateFailed(Guid vehicleId) => Error.Failure(
        "Vehicles.UpdateFailed",
        $"Failed to update vehicle with Id = '{vehicleId}'");
    
    public static Error DeleteFailed(Guid vehicleId) => Error.Failure(
        "Vehicles.DeleteFailed",
        $"Failed to delete vehicle with Id = '{vehicleId}'");
    
    public static Error NotFoundForUser(Guid userId) => Error.NotFound(
        "Vehicles.NotFound",
        $"Not found any vehicles with the UserId = '{userId}'");
    
    public static Error NotFound(Guid vehicleId) => Error.NotFound(
        "Vehicles.NotFound",
        $"Not found vehicle with Id = '{vehicleId}'");
}