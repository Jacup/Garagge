using Domain.Entities.EnergyEntries;

namespace Application.Services.EnergyStats;

public interface IEnergyStatsService
{
    decimal CalculateAverageConsumption(IReadOnlyCollection<EnergyEntry> entries);
    decimal CalculateTotalVolume(IReadOnlyCollection<EnergyEntry> entries);
    decimal CalculateTotalCost(IReadOnlyCollection<EnergyEntry> entries);
    decimal CalculateAveragePricePerUnit(IReadOnlyCollection<EnergyEntry> entries);
}

