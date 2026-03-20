using Domain.Enums;

namespace Application.Features.EnergyEntries;

public record EnergyTypeStatsDto(
    EnergyType Type,
    int ItemsCount,
    decimal TotalCost,
    decimal TotalVolume,
    decimal AverageConsumption,
    decimal AveragePricePerUnit,
    decimal AverageCostPer100km
);