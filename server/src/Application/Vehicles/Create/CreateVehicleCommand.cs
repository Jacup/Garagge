using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Vehicles.Create;

public sealed record CreateVehicleCommand(
    string Brand,
    string Model,
    EngineType EngineType,
    IEnumerable<EnergyType> EnergyTypes,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null)
    : ICommand<VehicleDto>;