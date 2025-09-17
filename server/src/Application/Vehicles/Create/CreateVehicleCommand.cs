using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Vehicles.Create;

public sealed record CreateVehicleCommand(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null,
    IEnumerable<EnergyType>? EnergyTypes = null)
    : ICommand<VehicleDto>;