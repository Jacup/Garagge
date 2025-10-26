using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace Application.Services.EnergyStats;

public interface IEnergyStatsService
{
    EnergyUnitStats CalculateStatisticsForUnit(EnergyUnit unit, IReadOnlyCollection<EnergyEntry> entries);
    
    decimal CalculateAverageConsumption(IReadOnlyCollection<EnergyEntry> entries);
    decimal CalculateTotalVolume(IReadOnlyCollection<EnergyEntry> entries);
    decimal CalculateTotalCost(IReadOnlyCollection<EnergyEntry> entries);
    decimal CalculateAveragePricePerUnit(IReadOnlyCollection<EnergyEntry> entries);
}

