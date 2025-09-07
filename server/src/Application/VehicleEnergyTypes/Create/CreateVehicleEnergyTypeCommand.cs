using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.VehicleEnergyTypes.Create;

public sealed record CreateVehicleEnergyTypeCommand(
    Guid VehicleId,
    EnergyType EnergyType)
    : ICommand<VehicleEnergyTypeDto>;
