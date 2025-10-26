using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace Application.Services.EnergyStats;

internal sealed class EnergyStatsService : IEnergyStatsService
{
    public EnergyUnitStats CalculateStatisticsForUnit(EnergyUnit unit, IReadOnlyCollection<EnergyEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        
        var includedTypes = entries
            .Select(e => e.Type)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        if (entries.Count < 2)
        {
            return new EnergyUnitStats
            {
                Unit = unit,
                EnergyTypes = includedTypes,
                EntriesCount = entries.Count,
                TotalVolume = CalculateTotalVolume(entries),
                TotalCost = CalculateTotalCost(entries),
                AverageConsumption = 0,
                AveragePricePerUnit = CalculateAveragePricePerUnit(entries),
                AverageCostPer100km = 0
            };
        }

        var avgConsumption = CalculateAverageConsumption(entries);
        var avgPricePerUnit = CalculateAveragePricePerUnit(entries);

        var stats = new EnergyUnitStats
        {
            Unit = unit,
            EnergyTypes = includedTypes,
            
            EntriesCount = entries.Count,
            TotalVolume = CalculateTotalVolume(entries),
            TotalCost = CalculateTotalCost(entries),
            
            AverageConsumption = avgConsumption,
            AveragePricePerUnit = avgPricePerUnit,
            AverageCostPer100km = avgConsumption > 0 && avgPricePerUnit > 0
                ? (avgConsumption / 100) * avgPricePerUnit
                : 0
        };

        return stats;
    }

    public decimal CalculateAverageConsumption(IReadOnlyCollection<EnergyEntry> entries)
    {
        var sortedEntries = entries.OrderBy(e => e.Mileage).ToList();
        
        if (sortedEntries.Count < 2)
            return 0;
        
        var consumptions = new List<decimal>();
        
        for (int i = 1; i < sortedEntries.Count; i++)
        {
            var previousEntry = sortedEntries[i - 1];
            var currentEntry = sortedEntries[i];
            
            var distanceTraveled = currentEntry.Mileage - previousEntry.Mileage;
            
            if (distanceTraveled <= 0)
                continue;
            
            var consumption = (currentEntry.Volume / distanceTraveled) * 100;
            consumptions.Add(consumption);
        }
        
        return consumptions.Count > 0 ? consumptions.Average() : 0;
    }

    public decimal CalculateTotalVolume(IReadOnlyCollection<EnergyEntry> entries)
    {
        return entries.Sum(e => e.Volume);
    }

    public decimal CalculateTotalCost(IReadOnlyCollection<EnergyEntry> entries)
    {
        return entries.Where(e => e.Cost != null).Sum(e => e.Cost!.Value);
    }

    public decimal CalculateAveragePricePerUnit(IReadOnlyCollection<EnergyEntry> entries)
    {
        var entriesWithPrice = entries.Where(e => e.PricePerUnit != null).ToList();
        
        if (entriesWithPrice.Count == 0)
            return 0;
        
        return entriesWithPrice.Average(e => e.PricePerUnit!.Value);
    }
}

