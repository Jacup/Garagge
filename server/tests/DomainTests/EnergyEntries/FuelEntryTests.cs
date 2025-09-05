using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace DomainTests.EnergyEntries;

public class EnergyEntryTests
{
    private readonly DateOnly _date = new(2024, 1, 15);
    private const int Mileage = 50000;
    private const decimal Cost = 120.50m;
    private readonly Guid _vehicleId = Guid.NewGuid();
    private const decimal Volume = 45.5m;
    private const EnergyUnit Unit = EnergyUnit.Liter;
    private const EnergyType Type = EnergyType.Gasoline;
    private const decimal PricePerUnit = 2.65m;

    [Fact]
    public void Constructor_ValidProperties_CreatesEntityWithCorrectData()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
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
    public void Constructor_ValidPropertiesWithNullOptionalFields_CreatesEntityWithCorrectData()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
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
    public void Setters_PropertiesUpdated_UpdatesDataCorrectly()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            Cost = Cost,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type,
            PricePerUnit = PricePerUnit
        };

        var newDate = new DateOnly(2024, 2, 20);
        const int newMileage = 55000;
        const decimal newCost = 135.75m;
        var newVehicleId = Guid.NewGuid();
        const decimal newVolume = 50.2m;
        const EnergyUnit newUnit = EnergyUnit.kWh;
        const EnergyType newType = EnergyType.Electric;
        const decimal newPricePerUnit = 3.15m;

        energyEntry.Date = newDate;
        energyEntry.Mileage = newMileage;
        energyEntry.Cost = newCost;
        energyEntry.VehicleId = newVehicleId;
        energyEntry.Volume = newVolume;
        energyEntry.EnergyUnit = newUnit;
        energyEntry.Type = newType;
        energyEntry.PricePerUnit = newPricePerUnit;

        energyEntry.Date.ShouldBe(newDate);
        energyEntry.Mileage.ShouldBe(newMileage);
        energyEntry.Cost.ShouldBe(newCost);
        energyEntry.VehicleId.ShouldBe(newVehicleId);
        energyEntry.Volume.ShouldBe(newVolume);
        energyEntry.EnergyUnit.ShouldBe(newUnit);
        energyEntry.Type.ShouldBe(newType);
        energyEntry.PricePerUnit.ShouldBe(newPricePerUnit);
    }

    [Theory]
    [InlineData(EnergyUnit.Liter)]
    [InlineData(EnergyUnit.Gallon)]
    [InlineData(EnergyUnit.CubicMeter)]
    [InlineData(EnergyUnit.kWh)]
    public void EnergyUnit_ValidEnergyUnit_SetsUnitCorrectly(EnergyUnit energyUnit)
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = energyUnit,
            Type = Type
        };

        energyEntry.EnergyUnit.ShouldBe(energyUnit);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline)]
    [InlineData(EnergyType.Diesel)]
    [InlineData(EnergyType.LPG)]
    [InlineData(EnergyType.CNG)]
    [InlineData(EnergyType.Electric)]
    [InlineData(EnergyType.Hydrogen)]
    public void Type_ValidEnergyType_SetsTypeCorrectly(EnergyType energyType)
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = energyType
        };

        energyEntry.Type.ShouldBe(energyType);
    }

    [Fact]
    public void Type_FuelTypes_CorrectlySetsFuelEnergyTypes()
    {
        var fuelTypes = new[] { EnergyType.Gasoline, EnergyType.Diesel, EnergyType.LPG, EnergyType.CNG, EnergyType.Ethanol, EnergyType.Biofuel };

        foreach (var fuelType in fuelTypes)
        {
            var energyEntry = new EnergyEntry
            {
                Date = _date,
                Mileage = Mileage,
                VehicleId = _vehicleId,
                Volume = Volume,
                EnergyUnit = EnergyUnit.Liter,
                Type = fuelType
            };

            energyEntry.Type.ShouldBe(fuelType);
            (energyEntry.Type & EnergyType.AllFuels).ShouldNotBe(EnergyType.None);
        }
    }

    [Fact]
    public void Type_ElectricType_CorrectlySetsElectricEnergyType()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = EnergyUnit.kWh,
            Type = EnergyType.Electric
        };

        energyEntry.Type.ShouldBe(EnergyType.Electric);
        (energyEntry.Type & EnergyType.AllCharging).ShouldNotBe(EnergyType.None);
    }

    [Fact]
    public void Type_HydrogenType_IsPartOfFuelTypes()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = EnergyUnit.CubicMeter,
            Type = EnergyType.Hydrogen
        };

        energyEntry.Type.ShouldBe(EnergyType.Hydrogen);
        (energyEntry.Type & EnergyType.AllFuels).ShouldNotBe(EnergyType.None);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, EnergyUnit.Liter)]
    [InlineData(EnergyType.Diesel, EnergyUnit.Liter)]
    [InlineData(EnergyType.LPG, EnergyUnit.Liter)]
    [InlineData(EnergyType.CNG, EnergyUnit.CubicMeter)]
    [InlineData(EnergyType.Ethanol, EnergyUnit.Liter)]
    [InlineData(EnergyType.Biofuel, EnergyUnit.Liter)]
    [InlineData(EnergyType.Hydrogen, EnergyUnit.CubicMeter)]
    [InlineData(EnergyType.Electric, EnergyUnit.kWh)]
    public void Type_WithAppropriateMeasurementUnit_CreatesValidEntry(EnergyType energyType, EnergyUnit expectedUnit)
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = expectedUnit,
            Type = energyType
        };

        energyEntry.Type.ShouldBe(energyType);
        energyEntry.EnergyUnit.ShouldBe(expectedUnit);
    }

    [Fact]
    public void Id_CanBeSetManually_AcceptsValidGuid()
    {
        var customId = Guid.NewGuid();
        var energyEntry = new EnergyEntry
        {
            Id = customId,
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        energyEntry.Id.ShouldBe(customId);
    }

    [Fact]
    public void Vehicle_NavigationProperty_CanBeSetAndRetrieved()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        energyEntry.Vehicle.ShouldBeNull(); // Navigation property starts as null
        energyEntry.VehicleId.ShouldBe(_vehicleId);
    }

    [Theory]
    [InlineData(50.0, 1.50, 75.0)]
    [InlineData(25.5, 2.10, 53.55)]
    [InlineData(0.0, 1.00, 0.0)]
    public void Cost_CalculatedValues_AreConsistentWithVolumeAndPrice(decimal volume, decimal pricePerUnit, decimal expectedCost)
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
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

    [Fact]
    public void Date_ValidDateOnly_AcceptsDateCorrectly()
    {
        var futureDate = new DateOnly(2025, 12, 31);
        var pastDate = new DateOnly(2020, 1, 1);

        var futureEntry = new EnergyEntry
        {
            Date = futureDate,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        var pastEntry = new EnergyEntry
        {
            Date = pastDate,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        futureEntry.Date.ShouldBe(futureDate);
        pastEntry.Date.ShouldBe(pastDate);
    }

    [Fact]
    public void CreatedAndUpdatedDates_InheritsFromEntity_HasDefaultValues()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        energyEntry.CreatedDate.ShouldBe(default);
        energyEntry.UpdatedDate.ShouldBe(default);
    }

    [Fact]
    public void DomainEvents_InheritsFromEntity_StartsEmpty()
    {
        var energyEntry = new EnergyEntry
        {
            Date = _date,
            Mileage = Mileage,
            VehicleId = _vehicleId,
            Volume = Volume,
            EnergyUnit = Unit,
            Type = Type
        };

        energyEntry.DomainEvents.ShouldBeEmpty();
    }
}
