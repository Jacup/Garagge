using Domain.Enums;

namespace Application.EnergyEntries;

public record EnergyStatsDto(
    decimal TotalVolume,
    decimal AverageConsumption,
    decimal TotalCost,
    decimal AveragePricePerUnit,
    EnergyType[] EnergyTypes
);