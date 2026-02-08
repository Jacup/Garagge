using Application.Abstractions.Messaging;

namespace Application.Features.Vehicles.Delete;

public sealed record DeleteVehicleByIdCommand(Guid VehicleId) : ICommand;
