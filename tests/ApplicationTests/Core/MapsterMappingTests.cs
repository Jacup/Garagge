using Application.Vehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Mapster;

namespace ApplicationTests.Core;

public class MapsterMappingTests
{
    [Fact]
    public void Vehicle_ToVehicleDto_ShouldMapCorrectly()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            PowerType = PowerType.Gasoline,
            ManufacturedYear = 2010,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109186",
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = vehicle.Adapt<VehicleDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(vehicle.Id);
        dto.Brand.ShouldBe(vehicle.Brand);
        dto.Model.ShouldBe(vehicle.Model);
        dto.PowerType.ShouldBe(vehicle.PowerType);
        dto.ManufacturedYear.ShouldBe(vehicle.ManufacturedYear);
        dto.Type.ShouldBe(vehicle.Type);
        dto.VIN.ShouldBe(vehicle.VIN);
        dto.UserId.ShouldBe(vehicle.UserId);
        dto.CreatedDate.ShouldBe(vehicle.CreatedDate);
        dto.UpdatedDate.ShouldBe(vehicle.UpdatedDate);
    }

    [Fact]
    public void Vehicle_WithMinimalData_ToVehicleDto_ShouldMapCorrectly()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Tesla",
            Model = "Model 3",
            PowerType = PowerType.Electric,
            ManufacturedYear = null,
            Type = null,
            VIN = null,
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = vehicle.Adapt<VehicleDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(vehicle.Id);
        dto.Brand.ShouldBe(vehicle.Brand);
        dto.Model.ShouldBe(vehicle.Model);
        dto.PowerType.ShouldBe(vehicle.PowerType);
        dto.ManufacturedYear.ShouldBeNull();
        dto.Type.ShouldBeNull();
        dto.VIN.ShouldBeNull();
        dto.UserId.ShouldBe(vehicle.UserId);
    }

    [Theory]
    [InlineData(PowerType.Gasoline)]
    [InlineData(PowerType.Diesel)]
    [InlineData(PowerType.Hybrid)]
    [InlineData(PowerType.PlugInHybrid)]
    [InlineData(PowerType.Electric)]
    public void Vehicle_WithDifferentPowerTypes_ToVehicleDto_ShouldMapCorrectly(PowerType powerType)
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Test",
            Model = "Test",
            PowerType = powerType,
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = vehicle.Adapt<VehicleDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.PowerType.ShouldBe(powerType);
    }

    [Theory]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Truck)]
    [InlineData(null)]
    public void Vehicle_WithDifferentVehicleTypes_ToVehicleDto_ShouldMapCorrectly(VehicleType? vehicleType)
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Test",
            Model = "Test",
            PowerType = PowerType.Gasoline,
            Type = vehicleType,
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = vehicle.Adapt<VehicleDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Type.ShouldBe(vehicleType);
    }

    [Fact]
    public void Vehicle_WithNavigationProperties_ToVehicleDto_ShouldMapWithoutNavigationProperties()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "BMW",
            Model = "X5",
            PowerType = PowerType.Diesel,
            UserId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            // Navigation properties - should not be mapped to DTO
            User = null,
            EnergyEntries = new List<Domain.Entities.EnergyEntries.EnergyEntry>()
        };

        // Act
        var dto = vehicle.Adapt<VehicleDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Brand.ShouldBe("BMW");
        dto.Model.ShouldBe("X5");
        dto.PowerType.ShouldBe(PowerType.Diesel);
        dto.UserId.ShouldBe(vehicle.UserId);
    }
}
