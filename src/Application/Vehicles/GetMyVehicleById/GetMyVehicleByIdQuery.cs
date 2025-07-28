using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetMyVehicleById;

public sealed record GetMyVehicleByIdQuery(Guid UserId, Guid VehicleId) : IQuery<VehicleDto>;
