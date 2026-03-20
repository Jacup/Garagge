using Application.Features.EnergyEntries;
using Domain.Entities.EnergyEntries;

namespace Application.Services.EnergyStats;

internal sealed class EnergyStatsService : IEnergyStatsService
{
    public decimal CalculateTotalCost(IReadOnlyCollection<EnergyEntry> entries)
    {
        return entries.Where(e => e.Cost != null).Sum(e => e.Cost!.Value);
    }

    public int CalculateDistanceDriven(IReadOnlyCollection<EnergyEntry> entries)
    {
        var firstEntry = entries.OrderBy(e => e.Mileage).FirstOrDefault();
        var lastEntry = entries.OrderByDescending(e => e.Mileage).FirstOrDefault();

        return firstEntry != null && lastEntry != null
            ? Math.Max(0, lastEntry.Mileage - firstEntry.Mileage)
            : 0;
    }
    
    public EnergyTypeStatsDto[] CalculateStatsByType(IReadOnlyCollection<EnergyEntry> entries)
    {
        return entries
            .GroupBy(e => e.Type)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var typeEntries = g.ToList();
                var avgConsumption = CalculateAverageConsumption(typeEntries);
                var avgPricePerUnit = CalculateAveragePricePerUnit(typeEntries);

                return new EnergyTypeStatsDto(
                    Type: g.Key,
                    ItemsCount: typeEntries.Count,
                    TotalCost: CalculateTotalCost(typeEntries),
                    TotalVolume: CalculateTotalVolume(typeEntries),
                    AverageConsumption: avgConsumption,
                    AveragePricePerUnit: avgPricePerUnit,
                    AverageCostPer100km: avgConsumption > 0 && avgPricePerUnit > 0
                        ? avgConsumption / 100 * avgPricePerUnit
                        : 0
                );
            })
            .OrderBy(e => e.ItemsCount)
            .ToArray();
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

    public decimal CalculateAveragePricePerUnit(IReadOnlyCollection<EnergyEntry> entries)
    {
        var entriesWithPrice = entries.Where(e => e.PricePerUnit != null).ToList();

        if (entriesWithPrice.Count == 0)
            return 0;

        return entriesWithPrice.Average(e => e.PricePerUnit!.Value);
    }
    
    public decimal CalculateTotalVolume(IReadOnlyCollection<EnergyEntry> entries)
    {
        return entries.Sum(e => e.Volume);
    }
}