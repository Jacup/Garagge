using Domain.Entities.EnergyEntries;

namespace Application.Abstractions;

public interface IEnergyEntryMileageValidator
{
    bool IsValid(IReadOnlyCollection<EnergyEntry> entries, EnergyEntry entryToValidate, DateOnly date, int mileage);
}