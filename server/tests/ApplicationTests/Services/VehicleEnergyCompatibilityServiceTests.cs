using Application.Services;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace ApplicationTests.Services;

public class VehicleEnergyCompatibilityServiceTests : InMemoryDbTestBase
{
    private readonly VehicleEnergyCompatibilityService _service;

    public VehicleEnergyCompatibilityServiceTests()
    {
        _service = new VehicleEnergyCompatibilityService(Context);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    [InlineData(EnergyType.Electric, false)]
    public async Task IsEnergyTypeCompatibleAsync_GasolineOnlyVehicle_ReturnsValidValue(EnergyType energyType, bool isCompatible)
    {
        // Arrange
        var vehicle = await CreateGasolineVehicleInDb();

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, energyType);

        // Assert
        result.ShouldBe(isCompatible);
    }
    
    
    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    [InlineData(EnergyType.Electric, false)]
    public async Task IsEnergyTypeCompatibleAsync_GasolineAndLpgOnlyVehicle_ReturnsValidValue(EnergyType energyType, bool isCompatible)
    {
        // Arrange
        var vehicle = await CreateGasolineAndLPGVehicleInDb();

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, energyType);

        // Assert
        result.ShouldBe(isCompatible);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, false)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    [InlineData(EnergyType.Electric, true)]
    public async Task IsEnergyTypeCompatibleAsync_EVVehicle_ReturnsValidValue(EnergyType energyType, bool isCompatible)
    {
        // Arrange
        var vehicle = await CreateEVVehicleInDb();

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, energyType);

        // Assert
        result.ShouldBe(isCompatible);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    [InlineData(EnergyType.Electric, true)]
    public async Task IsEnergyTypeCompatibleAsync_PHEVVehicle_ReturnsValidValue(EnergyType energyType, bool isCompatible)
    {
        // Arrange
        var vehicle = await CreatePHEVVehicleInDb();

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, energyType);

        // Assert
        result.ShouldBe(isCompatible);
    }

    private async Task<Vehicle> CreateGasolineVehicleInDb()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Test Car",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.Gasoline });

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private async Task<Vehicle> CreateGasolineAndLPGVehicleInDb()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Test Car",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.Gasoline });
        vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.LPG });

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }
    
    private async Task<Vehicle> CreateEVVehicleInDb()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Test Car",
            EngineType = EngineType.Electric,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.Electric });

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private async Task<Vehicle> CreatePHEVVehicleInDb()
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Test Car",
            EngineType = EngineType.PlugInHybrid,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.Electric });
        vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.Gasoline });

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }
}