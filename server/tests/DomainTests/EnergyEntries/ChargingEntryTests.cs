using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace DomainTests.EnergyEntries;

public class ChargingEnergyEntryTests
{
    private readonly DateOnly _date = new(2024, 1, 15);
    private const int Mileage = 50000;
    private const decimal Cost = 85.30m;
    private readonly Guid _vehicleId = Guid.NewGuid();
    private const decimal Volume = 42.5m;
    private const EnergyUnit Unit = EnergyUnit.kWh;
    private const EnergyType Type = EnergyType.Electric;
    private const decimal PricePerUnit = 2.01m;

    [Fact]
    public void Constructor_ValidElectricEnergyEntry_CreatesEntityWithCorrectData()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            Vehicle = null!,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type,
            PricePerUnit = PricePerUnit
        };

        energyEntry.Date.ShouldBe(_date);
        energyEntry.Mileage.ShouldBe(Mileage);
        energyEntry.Cost.ShouldBe(Cost);
        energyEntry.VehicleId.ShouldBe(_vehicleId);
        energyEntry.Volume.ShouldBe(Volume);
        energyEntry.EnergyUnit.ShouldBe(Unit);
        energyEntry.Type.ShouldBe(Type);
        energyEntry.PricePerUnit.ShouldBe(PricePerUnit);
    }

    [Fact]
    public void Constructor_ElectricEnergyWithoutOptionalFields_CreatesEntityWithCorrectData()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Vehicle = null!,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        energyEntry.Date.ShouldBe(_date);
        energyEntry.Mileage.ShouldBe(Mileage);
        energyEntry.VehicleId.ShouldBe(_vehicleId);
        energyEntry.Volume.ShouldBe(Volume);
        energyEntry.EnergyUnit.ShouldBe(Unit);
        energyEntry.Type.ShouldBe(Type);
        energyEntry.Cost.ShouldBeNull();
        energyEntry.PricePerUnit.ShouldBeNull();
    }

    [Fact]
    public void EnergyUnit_ElectricType_DefaultsToKwh()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Vehicle = null!,
            Volume = Volume,
            EnergyUnit = EnergyUnit.kWh,
            Type = EnergyType.Electric
        };

        energyEntry.EnergyUnit.ShouldBe(EnergyUnit.kWh);
        energyEntry.Type.ShouldBe(EnergyType.Electric);
    }
    
    [Theory]
    [InlineData(42.5, EnergyUnit.kWh)]
    [InlineData(150.0, EnergyUnit.kWh)]
    [InlineData(25.8, EnergyUnit.kWh)]
    public void Volume_ElectricEnergyWithKwh_AcceptsValidValues(decimal volume, EnergyUnit unit)
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Vehicle = null!,
            Volume = volume,
            EnergyUnit = unit,
            Type = EnergyType.Electric
        };

        energyEntry.Volume.ShouldBe(volume);
        energyEntry.EnergyUnit.ShouldBe(unit);
    }

    [Fact]
    public void Vehicle_NavigationProperty_CanBeSetAndRetrieved()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Vehicle = null!,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        energyEntry.Vehicle.ShouldBeNull();
        energyEntry.VehicleId.ShouldBe(_vehicleId);
    }

    [Fact]
    public void Cost_CalculatedFromVolumeAndPrice_ReturnsCorrectValue()
    {
        const decimal volume = 50.0m;
        const decimal pricePerUnit = 0.12m;
        const decimal expectedCost = 6.0m;

        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Vehicle = null!,
            Volume = volume,
            EnergyUnit = Unit,
            Type = Type,
            PricePerUnit = pricePerUnit,
            Cost = expectedCost
        };

        energyEntry.Cost.ShouldBe(expectedCost);
        energyEntry.PricePerUnit.ShouldBe(pricePerUnit);
        energyEntry.Volume.ShouldBe(volume);
    }
}
