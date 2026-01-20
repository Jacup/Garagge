namespace Application.Vehicles.Stats;

public record VehicleStatsDto(
    Guid VehicleId,
    decimal TotalCost,
    int LastMileage,
    int DistanceTraveled,
    decimal TotalFuelCost,
    decimal FuelCostPerKm,
    int TotalFuelEntries,
    DateOnly? LastFuelEntryDate,
    decimal TotalServicesCost,
    int TotalServiceRecords,
    DateOnly? LastServiceDate,
    List<EnergyEfficiencyStatDto> EfficiencyStats,
    VehicleActivityDto[] VehicleActivities
);