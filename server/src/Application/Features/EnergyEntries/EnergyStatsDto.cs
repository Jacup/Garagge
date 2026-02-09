using Domain.Entities.EnergyEntries;

namespace Application.Features.EnergyEntries;

public record EnergyStatsDto(
    Guid VehicleId,
    decimal TotalCost,
    int TotalEntries,
    EnergyUnitStats[] EnergyUnitStats
);