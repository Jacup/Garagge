using Domain.Enums;

namespace Application.Features.Vehicles.Stats;

public record EnergyEfficiencyStatDto(
    EnergyType FuelType,
    string EnergyUnit,
    double AverageConsumption,
    decimal CostPerKm,
    decimal TotalCost,
    int EntriesCount
);