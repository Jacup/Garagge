using Application.Abstractions;
using Application.Abstractions.Services;
using Domain.Entities.EnergyEntries;

namespace Application.Services;

public class EnergyEntryMileageValidator : IEnergyEntryMileageValidator
{
    public bool IsValid(IReadOnlyCollection<EnergyEntry> entries, EnergyEntry entryToValidate, DateOnly date, int mileage)
    {
        var laterEntryWithLowerMileage = entries.Any(ee => 
            ee.VehicleId == entryToValidate.VehicleId && 
            ee.Id != entryToValidate.Id && 
            ee.Date > date && 
            ee.Mileage < mileage);

        var earlierEntryWithHigherMileage = entries.Any(ee => 
            ee.VehicleId == entryToValidate.VehicleId &&
            ee.Id != entryToValidate.Id && 
            ee.Date < date && 
            ee.Mileage > mileage);

        return !laterEntryWithLowerMileage && !earlierEntryWithHigherMileage;
    }
}