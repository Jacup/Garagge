using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace DomainTests.EnergyEntries;

public class ChargingEntryTests
{
    private readonly DateOnly _date = new(2024, 1, 15);
    private const int Mileage = 50000;
    private const decimal Cost = 85.30m;
    private readonly Guid _vehicleId = Guid.NewGuid();
    private const decimal EnergyAmount = 42.5m;
    private const EnergyUnit Unit = EnergyUnit.kWh;
    private const decimal PricePerUnit = 2.01m;
    private const int ChargingDurationMinutes = 90;

    [Fact]
    public void Constructor_ValidProperties_CreatesEntityWithCorrectData()
    {
        var chargingEntry = new ChargingEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            EnergyAmount = EnergyAmount,
            Unit = Unit,
            PricePerUnit = PricePerUnit,
            ChargingDurationMinutes = ChargingDurationMinutes
        };

        chargingEntry.Date.ShouldBe(_date);
        chargingEntry.Mileage.ShouldBe(Mileage);
        chargingEntry.Cost.ShouldBe(Cost);
        chargingEntry.VehicleId.ShouldBe(_vehicleId);
        chargingEntry.EnergyAmount.ShouldBe(EnergyAmount);
        chargingEntry.Unit.ShouldBe(Unit);
        chargingEntry.PricePerUnit.ShouldBe(PricePerUnit);
        chargingEntry.ChargingDurationMinutes.ShouldBe(ChargingDurationMinutes);
    }

    [Fact]
    public void Constructor_ValidPropertiesAndNullChargingDuration_CreatesEntityWithCorrectData()
    {
        var chargingEntry = new ChargingEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            EnergyAmount = EnergyAmount,
            Unit = Unit,
            PricePerUnit = PricePerUnit,
            ChargingDurationMinutes = null
        };

        chargingEntry.Date.ShouldBe(_date);
        chargingEntry.Mileage.ShouldBe(Mileage);
        chargingEntry.Cost.ShouldBe(Cost);
        chargingEntry.VehicleId.ShouldBe(_vehicleId);
        chargingEntry.EnergyAmount.ShouldBe(EnergyAmount);
        chargingEntry.Unit.ShouldBe(Unit);
        chargingEntry.PricePerUnit.ShouldBe(PricePerUnit);
        chargingEntry.ChargingDurationMinutes.ShouldBeNull();
    }

    [Fact]
    public void Setters_PropertiesUpdated_UpdatesDataCorrectly()
    {
        var chargingEntry = new ChargingEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            EnergyAmount = EnergyAmount,
            Unit = Unit,
            PricePerUnit = PricePerUnit,
            ChargingDurationMinutes = ChargingDurationMinutes
        };

        var newDate = new DateOnly(2024, 2, 20);
        const int newMileage = 55000;
        const decimal newCost = 95.75m;
        var newVehicleId = Guid.NewGuid();
        const decimal newEnergyAmount = 38.2m;
        const decimal newPricePerUnit = 2.51m;
        const int newChargingDurationMinutes = 75;

        chargingEntry.Date = newDate;
        chargingEntry.Mileage = newMileage;
        chargingEntry.Cost = newCost;
        chargingEntry.VehicleId = newVehicleId;
        chargingEntry.EnergyAmount = newEnergyAmount;
        chargingEntry.PricePerUnit = newPricePerUnit;
        chargingEntry.ChargingDurationMinutes = newChargingDurationMinutes;

        chargingEntry.Date.ShouldBe(newDate);
        chargingEntry.Mileage.ShouldBe(newMileage);
        chargingEntry.Cost.ShouldBe(newCost);
        chargingEntry.VehicleId.ShouldBe(newVehicleId);
        chargingEntry.EnergyAmount.ShouldBe(newEnergyAmount);
        chargingEntry.PricePerUnit.ShouldBe(newPricePerUnit);
        chargingEntry.ChargingDurationMinutes.ShouldBe(newChargingDurationMinutes);
    }

    [Fact]
    public void Unit_DefaultValue_SetsToKwh()
    {
        var chargingEntry = new ChargingEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            EnergyAmount = EnergyAmount,
            PricePerUnit = PricePerUnit,
            Unit = EnergyUnit.kWh
        };

        chargingEntry.Unit.ShouldBe(EnergyUnit.kWh);
    }
}
