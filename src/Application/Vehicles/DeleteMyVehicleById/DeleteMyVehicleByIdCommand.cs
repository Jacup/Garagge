using Application.Abstractions.Messaging;

namespace Application.Vehicles.DeleteMyVehicleById;

public sealed record DeleteMyVehicleByIdCommand(Guid VehicleId) : ICommand<bool>
{
}

