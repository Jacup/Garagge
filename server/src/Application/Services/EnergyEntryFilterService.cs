using Application.Abstractions.Services;
using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace Application.Services;

internal sealed class EnergyEntryFilterService : IEnergyEntryFilterService
{
    public IQueryable<EnergyEntry> ApplyEnergyTypeFilter(IQueryable<EnergyEntry> query, EnergyType? energyType)
    {
        if (energyType is null or EnergyType.None)
            return query;

        return query.Where(ee => (energyType.Value & ee.Type) != 0);
    }

    public IQueryable<EnergyEntry> ApplyUserFilter(IQueryable<EnergyEntry> query, Guid userId)
    {
        return query.Where(e => e.Vehicle!.UserId == userId);
    }

    public IQueryable<EnergyEntry> ApplyVehicleFilter(IQueryable<EnergyEntry> query, Guid vehicleId)
    {
        return query.Where(e => e.VehicleId == vehicleId);
    }

    public IQueryable<EnergyEntry> ApplyDefaultSorting(IQueryable<EnergyEntry> query)
    {
        return query
            .OrderByDescending(ee => ee.Date)
            .ThenByDescending(ee => ee.Mileage);
    }
}
