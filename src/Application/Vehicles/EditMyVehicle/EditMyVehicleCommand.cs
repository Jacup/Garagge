using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Vehicles.EditMyVehicle;

public sealed record EditMyVehicleCommand(
    Guid VehicleId,
    string Brand,
    string Model,
    PowerType PowerType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null)
    : ICommand<VehicleDto>;