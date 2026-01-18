using Domain.Enums;

namespace Application.Vehicles.Stats;

public record EnergyEfficiencyStatDto(
    EnergyType FuelType,
    string EnergyUnit,
    double AverageConsumption,
    decimal CostPerKm,
    decimal TotalCost,
    int EntriesCount
);