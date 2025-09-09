using Application.Abstractions.Messaging;

namespace Application.VehicleEnergyTypes.Delete;

public sealed record DeleteVehicleEnergyTypeCommand(Guid Id, Guid VehicleId) : ICommand;