using Domain.Entities.EnergyEntries;

namespace Application.EnergyEntries;

public record EnergyStatsDto(
    Guid VehicleId,
    decimal TotalCost,
    int TotalEntries,
    EnergyUnitStats[] EnergyUnitStats
);