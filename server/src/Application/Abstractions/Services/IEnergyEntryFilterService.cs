using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IEnergyEntryFilterService
{
    IQueryable<EnergyEntry> ApplyEnergyTypeFilter(IQueryable<EnergyEntry> query, EnergyType? energyType);
    IQueryable<EnergyEntry> ApplyUserFilter(IQueryable<EnergyEntry> query, Guid userId);
    IQueryable<EnergyEntry> ApplyVehicleFilter(IQueryable<EnergyEntry> query, Guid vehicleId);
    IQueryable<EnergyEntry> ApplyDefaultSorting(IQueryable<EnergyEntry> query);
}
