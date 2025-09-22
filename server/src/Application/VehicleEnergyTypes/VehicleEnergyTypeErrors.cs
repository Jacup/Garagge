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

    public static Error NotFound(Guid id) => Error.Failure(
        "VehicleEnergyType.NotFound",
        $"Not found vehicle energy type with ID = '{id}'");
    
    public static Error VehicleNotFound(Guid id) => Error.Failure(
        "VehicleEnergyType.VehicleNotFound",
        $"Not found vehicle with ID = '{id}'");

    public static Error AlreadyExists(Guid id, EnergyType energyType) => Error.Conflict(
        "VehicleEnergyType.AlreadyExists",
        $"Vehicle energy type with VehicleId = '{id}' and EnergyType = '{energyType}' already exists");

    public static Error DeleteFailed(Guid id) => Error.Failure(
        "VehicleEnergyType.DeleteFailed",
        $"Failed to delete vehicle entry type with Id = '{id}'");

    public static Error DeleteFailedEntriesExists(Guid id) => Error.Failure(
        "VehicleEnergyType.DeleteFailedEntriesExists",
        $"Failed to delete vehicle entry type with Id = '{id}'. There are energy entries that need to be deleted first.");

    public static Error IncompatibleWithEngine(EnergyType requestEnergyType, EngineType engineType) => Error.Failure(
        "VehicleEnergyType.IncompatibleWithEngine",
        $"EnergyType '{requestEnergyType}' is incompatible with EngineType '{engineType}'");
}