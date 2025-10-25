using Domain.Entities.EnergyEntries;

namespace Application.Services.EnergyStats;

internal sealed class EnergyStatsService : IEnergyStatsService
{
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

