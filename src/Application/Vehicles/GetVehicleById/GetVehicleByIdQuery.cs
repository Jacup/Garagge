using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetVehicleById;

public sealed record GetVehicleByIdQuery(Guid UserId, Guid VehicleId) : IQuery<VehicleDto>;
