namespace Application.Features.EnergyEntries;

public record EnergyStatsDto(
    Guid VehicleId,
    decimal TotalFuelCost,
    int TotalEntries,
    int DistanceDriven,
    EnergyTypeStatsDto[] StatsByType,
    EnergyEntryDto[] ChartEntries
);