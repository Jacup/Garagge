using Application.Abstractions.Messaging;

namespace Application.Features.Vehicles.Stats.GetByVehicleId;

public sealed record GetStatsByVehicleIdQuery(Guid VehicleId) : IQuery<VehicleStatsDto>;