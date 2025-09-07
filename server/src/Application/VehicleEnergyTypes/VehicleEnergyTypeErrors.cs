using Application.Core;
using Domain.Enums;

namespace Application.VehicleEnergyTypes;

public static class VehicleEnergyTypeErrors
{
    public static Error Unauthorized => Error.Unauthorized(
        "VehicleEnergyType.Unauthorized",
        "You are not authorized to perform this action");
    
    public static Error CreateFailed => Error.Failure(
        "VehicleEnergyType.CreateFailed",
        "Failed to create vehicle energy type");
    
    public static Error AlreadyExists(Guid vehicleId, EnergyType energyType) => Error.Conflict(
        "VehicleEnergyType.AlreadyExists",
        $"Vehicle energy type with VehicleId = '{vehicleId}' and EnergyType = '{energyType}' already exists");
    
    public static Error DeleteFailed(Guid vehicleId) => Error.Failure(
        "VehicleEnergyType.DeleteFailed",
        $"Failed to delete vehicle entry type with Id = '{vehicleId}'");    
    
    public static Error IncompatibleWithEngine(EnergyType requestEnergyType, EngineType engineType) => Error.Failure(
        "VehicleEnergyType.DeleteFailed",
        $"EnergyType '{requestEnergyType}' is incompatible with EngineType '{engineType}'");
}