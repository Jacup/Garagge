using Domain.Entities.EnergyEntries;

namespace Application.Abstractions.Services;

public interface IEnergyEntryMileageValidator
{
    bool IsValid(IReadOnlyCollection<EnergyEntry> entries, EnergyEntry entryToValidate, DateOnly date, int mileage);
}