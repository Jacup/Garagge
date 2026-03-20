using Application.Abstractions.Messaging;
using Domain.Enums.Energy;

namespace Application.Features.EnergyEntries.GetStats;

public sealed record GetEnergyStatsQuery(Guid VehicleId, StatsPeriod Period) : IQuery<EnergyStatsDto>;