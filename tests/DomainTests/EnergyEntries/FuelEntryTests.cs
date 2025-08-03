using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace DomainTests.EnergyEntries;

public class FuelEntryTests
{
    private readonly DateOnly _date = new(2024, 1, 15);
    private const int Mileage = 50000;
    private const decimal Cost = 120.50m;
    private readonly Guid _vehicleId = Guid.NewGuid();
    private const decimal Volume = 45.5m;
    private const VolumeUnit Unit = VolumeUnit.Liters;
    private const decimal PricePerUnit = 2.65m;

    [Fact]
    public void Constructor_ValidProperties_CreatesEntityWithCorrectData()
    {
        var fuelEntry = new FuelEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            Volume = Volume,
            Unit = Unit,
            PricePerUnit = PricePerUnit
        };

        fuelEntry.Date.ShouldBe(_date);
        fuelEntry.Mileage.ShouldBe(Mileage);
        fuelEntry.Cost.ShouldBe(Cost);
        fuelEntry.VehicleId.ShouldBe(_vehicleId);
        fuelEntry.Volume.ShouldBe(Volume);
        fuelEntry.Unit.ShouldBe(Unit);
        fuelEntry.PricePerUnit.ShouldBe(PricePerUnit);
    }

    [Fact]
    public void Setters_PropertiesUpdated_UpdatesDataCorrectly()
    {
        var fuelEntry = new FuelEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            Volume = Volume,
            Unit = Unit,
            PricePerUnit = PricePerUnit
        };

        var newDate = new DateOnly(2024, 2, 20);
        const int newMileage = 55000;
        const decimal newCost = 135.75m;
        var newVehicleId = Guid.NewGuid();
        const decimal newVolume = 50.2m;
        const VolumeUnit newUnit = VolumeUnit.Gallons;
        const decimal newPricePerUnit = 3.15m;

        fuelEntry.Date = newDate;
        fuelEntry.Mileage = newMileage;
        fuelEntry.Cost = newCost;
        fuelEntry.VehicleId = newVehicleId;
        fuelEntry.Volume = newVolume;
        fuelEntry.Unit = newUnit;
        fuelEntry.PricePerUnit = newPricePerUnit;

        fuelEntry.Date.ShouldBe(newDate);
        fuelEntry.Mileage.ShouldBe(newMileage);
        fuelEntry.Cost.ShouldBe(newCost);
        fuelEntry.VehicleId.ShouldBe(newVehicleId);
        fuelEntry.Volume.ShouldBe(newVolume);
        fuelEntry.Unit.ShouldBe(newUnit);
        fuelEntry.PricePerUnit.ShouldBe(newPricePerUnit);
    }

    [Theory]
    [InlineData(VolumeUnit.Liters)]
    [InlineData(VolumeUnit.Gallons)]
    public void Unit_ValidVolumeUnit_SetsUnitCorrectly(VolumeUnit volumeUnit)
    {
        var fuelEntry = new FuelEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            Volume = Volume,
            Unit = volumeUnit,
            PricePerUnit = PricePerUnit
        };

        fuelEntry.Unit.ShouldBe(volumeUnit);
    }
}
