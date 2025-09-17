using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Vehicles.Update;

public sealed record UpdateVehicleCommand(
    Guid VehicleId,
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null)
    : ICommand<VehicleDto>;