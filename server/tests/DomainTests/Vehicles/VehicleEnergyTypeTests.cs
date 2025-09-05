using Domain.Entities.Vehicles;
using Domain.Enums;

namespace DomainTests.Vehicles;

public class VehicleEnergyTypeTests
{
    private readonly Guid _vehicleId = Guid.NewGuid();
    private const EnergyType EnergyType = Domain.Enums.EnergyType.Gasoline;

    [Fact]
    public void Constructor_ValidProperties_CreatesEntityWithCorrectData()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType
        };

        vehicleEnergyType.VehicleId.ShouldBe(_vehicleId);
        vehicleEnergyType.EnergyType.ShouldBe(EnergyType);
        vehicleEnergyType.Id.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ValidPropertiesWithVehicleNavigation_CreatesEntityWithCorrectData()
    {
        var vehicle = new Vehicle
        {
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            UserId = Guid.NewGuid()
        };

        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = vehicle.Id,
            Vehicle = vehicle,
            EnergyType = EnergyType
        };

        vehicleEnergyType.VehicleId.ShouldBe(vehicle.Id);
        vehicleEnergyType.Vehicle.ShouldBe(vehicle);
        vehicleEnergyType.EnergyType.ShouldBe(EnergyType);
    }

    [Fact]
    public void Setters_PropertiesUpdated_UpdatesDataCorrectly()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType
        };

        var newVehicleId = Guid.NewGuid();
        const EnergyType newEnergyType = EnergyType.Electric;

        vehicleEnergyType.VehicleId = newVehicleId;
        vehicleEnergyType.EnergyType = newEnergyType;

        vehicleEnergyType.VehicleId.ShouldBe(newVehicleId);
        vehicleEnergyType.EnergyType.ShouldBe(newEnergyType);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline)]
    [InlineData(EnergyType.Diesel)]
    [InlineData(EnergyType.LPG)]
    [InlineData(EnergyType.CNG)]
    [InlineData(EnergyType.Ethanol)]
    [InlineData(EnergyType.Biofuel)]
    [InlineData(EnergyType.Hydrogen)]
    [InlineData(EnergyType.Electric)]
    public void EnergyType_ValidEnergyTypes_SetsEnergyTypeCorrectly(EnergyType energyType)
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = energyType
        };

        vehicleEnergyType.EnergyType.ShouldBe(energyType);
    }

    [Fact]
    public void EnergyType_FlagCombinations_SetsCorrectly()
    {
        var combinedType = EnergyType.Gasoline | EnergyType.Electric;
        
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = combinedType
        };

        vehicleEnergyType.EnergyType.ShouldBe(combinedType);
        (vehicleEnergyType.EnergyType & EnergyType.Gasoline).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.Electric).ShouldNotBe(EnergyType.None);
    }

    [Fact]
    public void EnergyType_AllFuelsFlag_MatchesIndividualFuelTypes()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType.AllFuels
        };

        (vehicleEnergyType.EnergyType & EnergyType.Gasoline).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.Diesel).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.LPG).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.CNG).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.Ethanol).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.Biofuel).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.Hydrogen).ShouldNotBe(EnergyType.None);
    }

    [Fact]
    public void EnergyType_AllChargingFlag_MatchesElectricType()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType.AllCharging
        };

        (vehicleEnergyType.EnergyType & EnergyType.Electric).ShouldNotBe(EnergyType.None);
        vehicleEnergyType.EnergyType.ShouldBe(EnergyType.Electric);
    }

    [Fact]
    public void EnergyType_AllFlag_ContainsAllEnergyTypes()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType.All
        };

        (vehicleEnergyType.EnergyType & EnergyType.AllFuels).ShouldNotBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.AllCharging).ShouldNotBe(EnergyType.None);
    }

    [Fact]
    public void EnergyType_NoneFlag_SetsToNone()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType.None
        };

        vehicleEnergyType.EnergyType.ShouldBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.AllFuels).ShouldBe(EnergyType.None);
        (vehicleEnergyType.EnergyType & EnergyType.AllCharging).ShouldBe(EnergyType.None);
    }

    [Fact]
    public void Vehicle_NavigationProperty_CanBeSetAndRetrieved()
    {
        var vehicle = new Vehicle
        {
            Brand = "BMW",
            Model = "i3",
            EngineType = EngineType.Electric,
            UserId = Guid.NewGuid()
        };

        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = vehicle.Id,
            EnergyType = EnergyType.Electric
        };

        vehicleEnergyType.Vehicle = vehicle;

        vehicleEnergyType.Vehicle.ShouldBe(vehicle);
        vehicleEnergyType.Vehicle.Id.ShouldBe(vehicle.Id);
    }

    [Fact]
    public void Vehicle_NavigationPropertyNull_DoesNotThrow()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType
        };

        vehicleEnergyType.Vehicle.ShouldBeNull();
        
        // Should not throw when accessing null navigation property
        Should.NotThrow(() => _ = vehicleEnergyType.Vehicle?.Brand);
    }

    [Fact]
    public void Id_CanBeSetManually_AcceptsValidGuid()
    {
        var customId = Guid.NewGuid();
        var vehicleEnergyType = new VehicleEnergyType
        {
            Id = customId,
            VehicleId = _vehicleId,
            EnergyType = EnergyType
        };

        vehicleEnergyType.Id.ShouldBe(customId);
    }

    [Fact]
    public void CreatedAndUpdatedDates_InheritsFromEntity_HasDefaultValues()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType
        };

        vehicleEnergyType.CreatedDate.ShouldBe(default(DateTime));
        vehicleEnergyType.UpdatedDate.ShouldBe(default(DateTime));
    }

    [Fact]
    public void DomainEvents_InheritsFromEntity_StartsEmpty()
    {
        var vehicleEnergyType = new VehicleEnergyType
        {
            VehicleId = _vehicleId,
            EnergyType = EnergyType
        };

        vehicleEnergyType.DomainEvents.ShouldBeEmpty();
    }
}