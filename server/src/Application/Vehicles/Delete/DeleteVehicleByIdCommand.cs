using Application.Abstractions.Messaging;

namespace Application.Vehicles.Delete;

public sealed record DeleteVehicleByIdCommand(Guid VehicleId) : ICommand;
