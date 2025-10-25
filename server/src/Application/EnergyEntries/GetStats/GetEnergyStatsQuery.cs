using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.EnergyEntries.GetStats;

public sealed record GetEnergyStatsQuery(Guid VehicleId, EnergyType[] EnergyTypes) : IQuery<EnergyStatsDto>;