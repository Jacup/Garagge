using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Vehicles.CreateMyVehicle;

public sealed record CreateMyVehicleCommand(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null)
    : ICommand<VehicleDto>;
