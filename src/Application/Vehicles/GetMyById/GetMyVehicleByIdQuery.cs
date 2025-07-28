using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetMyById;

public sealed record GetMyVehicleByIdQuery(Guid UserId, Guid VehicleId) : IQuery<VehicleDto>;
