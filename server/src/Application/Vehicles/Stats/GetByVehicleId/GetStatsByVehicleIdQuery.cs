using Application.Abstractions.Messaging;

namespace Application.Vehicles.Stats.GetByVehicleId;

public sealed record GetStatsByVehicleIdQuery(Guid VehicleId) : IQuery<VehicleStatsDto>;