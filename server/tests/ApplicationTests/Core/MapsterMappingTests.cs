using Application.EnergyEntries;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
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
            PowerType = EngineType.Fuel,
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
            Brand = "BMW",
            Model = "X3",
            PowerType = EngineType.Electric,
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
        dto.CreatedDate.ShouldBe(vehicle.CreatedDate);
        dto.UpdatedDate.ShouldBe(vehicle.UpdatedDate);
    }

    [Fact]
    public void EnergyEntry_ToEnergyEntryDto_ShouldMapCorrectly()
    {
        // Arrange
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2023, 10, 14),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50.5m,
            Cost = 120.75m,
            PricePerUnit = 2.40m,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = energyEntry.Adapt<EnergyEntryDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(energyEntry.Id);
        dto.VehicleId.ShouldBe(energyEntry.VehicleId);
        dto.Date.ShouldBe(energyEntry.Date);
        dto.Mileage.ShouldBe(energyEntry.Mileage);
        dto.Type.ShouldBe(energyEntry.Type);
        dto.EnergyUnit.ShouldBe(energyEntry.EnergyUnit);
        dto.Volume.ShouldBe(energyEntry.Volume);
        dto.Cost.ShouldBe(energyEntry.Cost);
        dto.PricePerUnit.ShouldBe(energyEntry.PricePerUnit);
        dto.CreatedDate.ShouldBe(energyEntry.CreatedDate);
        dto.UpdatedDate.ShouldBe(energyEntry.UpdatedDate);
    }

    [Fact]
    public void EnergyEntry_WithNullOptionalFields_ToEnergyEntryDto_ShouldMapCorrectly()
    {
        // Arrange
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2023, 10, 14),
            Mileage = 1000,
            Type = EnergyType.Electric,
            EnergyUnit = EnergyUnit.kWh,
            Volume = 30.0m,
            Cost = null,
            PricePerUnit = null,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = energyEntry.Adapt<EnergyEntryDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(energyEntry.Id);
        dto.VehicleId.ShouldBe(energyEntry.VehicleId);
        dto.Date.ShouldBe(energyEntry.Date);
        dto.Mileage.ShouldBe(energyEntry.Mileage);
        dto.Type.ShouldBe(energyEntry.Type);
        dto.EnergyUnit.ShouldBe(energyEntry.EnergyUnit);
        dto.Volume.ShouldBe(energyEntry.Volume);
        dto.Cost.ShouldBeNull();
        dto.PricePerUnit.ShouldBeNull();
        dto.CreatedDate.ShouldBe(energyEntry.CreatedDate);
        dto.UpdatedDate.ShouldBe(energyEntry.UpdatedDate);
    }

    [Fact]
    public void EnergyEntry_ElectricType_ToEnergyEntryDto_ShouldMapCorrectly()
    {
        // Arrange
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2023, 10, 14),
            Mileage = 5000,
            Type = EnergyType.Electric,
            EnergyUnit = EnergyUnit.kWh,
            Volume = 25.0m,
            Cost = 15.50m,
            PricePerUnit = 0.62m,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = energyEntry.Adapt<EnergyEntryDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Type.ShouldBe(EnergyType.Electric);
        dto.EnergyUnit.ShouldBe(EnergyUnit.kWh);
        dto.Volume.ShouldBe(25.0m);
    }

    [Fact]
    public void EnergyEntry_DieselType_ToEnergyEntryDto_ShouldMapCorrectly()
    {
        // Arrange
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2023, 10, 14),
            Mileage = 10000,
            Type = EnergyType.Diesel,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 45.0m,
            Cost = 70.0m,
            PricePerUnit = 1.56m,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // Act
        var dto = energyEntry.Adapt<EnergyEntryDto>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Type.ShouldBe(EnergyType.Diesel);
        dto.EnergyUnit.ShouldBe(EnergyUnit.Liter);
        dto.Volume.ShouldBe(45.0m);
    }

    [Fact]
    public void EnergyEntry_MultipleEnergyTypes_ShouldMapCorrectly()
    {
        // Arrange & Act & Assert
        var gasolineEntry = CreateEnergyEntry(EnergyType.Gasoline, EnergyUnit.Liter);
        var electricEntry = CreateEnergyEntry(EnergyType.Electric, EnergyUnit.kWh);
        var dieselEntry = CreateEnergyEntry(EnergyType.Diesel, EnergyUnit.Liter);
        var lpgEntry = CreateEnergyEntry(EnergyType.LPG, EnergyUnit.Liter);

        var gasolineDto = gasolineEntry.Adapt<EnergyEntryDto>();
        var electricDto = electricEntry.Adapt<EnergyEntryDto>();
        var dieselDto = dieselEntry.Adapt<EnergyEntryDto>();
        var lpgDto = lpgEntry.Adapt<EnergyEntryDto>();

        gasolineDto.Type.ShouldBe(EnergyType.Gasoline);
        gasolineDto.EnergyUnit.ShouldBe(EnergyUnit.Liter);

        electricDto.Type.ShouldBe(EnergyType.Electric);
        electricDto.EnergyUnit.ShouldBe(EnergyUnit.kWh);

        dieselDto.Type.ShouldBe(EnergyType.Diesel);
        dieselDto.EnergyUnit.ShouldBe(EnergyUnit.Liter);

        lpgDto.Type.ShouldBe(EnergyType.LPG);
        lpgDto.EnergyUnit.ShouldBe(EnergyUnit.Liter);
    }

    private static EnergyEntry CreateEnergyEntry(EnergyType energyType, EnergyUnit energyUnit)
    {
        return new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2023, 10, 14),
            Mileage = 1000,
            Type = energyType,
            EnergyUnit = energyUnit,
            Volume = 50.0m,
            Cost = 100.0m,
            PricePerUnit = 2.0m,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }
}
