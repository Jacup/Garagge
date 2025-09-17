using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetById;

public sealed record GetVehicleByIdQuery(Guid VehicleId) : IQuery<VehicleDto>;
