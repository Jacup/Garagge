using Application.Features.EnergyEntries;
using Domain.Entities.EnergyEntries;

namespace Application.Services.EnergyStats;

public interface IEnergyStatsService
{
    decimal CalculateTotalCost(IReadOnlyCollection<EnergyEntry> entries);

    int CalculateDistanceDriven(IReadOnlyCollection<EnergyEntry> entries);

    EnergyTypeStatsDto[] CalculateStatsByType(IReadOnlyCollection<EnergyEntry> entries);

    decimal CalculateAverageConsumption(IReadOnlyCollection<EnergyEntry> entries);

    decimal CalculateAveragePricePerUnit(IReadOnlyCollection<EnergyEntry> entries);

    decimal CalculateTotalVolume(IReadOnlyCollection<EnergyEntry> entries);
}