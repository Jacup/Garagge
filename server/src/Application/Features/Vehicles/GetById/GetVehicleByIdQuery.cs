using Application.Abstractions.Messaging;

namespace Application.Features.Vehicles.GetById;

public sealed record GetVehicleByIdQuery(Guid VehicleId) : IQuery<VehicleDto>;
