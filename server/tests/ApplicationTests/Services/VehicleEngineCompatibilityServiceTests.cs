using Application.Core;
using Application.Services;
using Application.VehicleEnergyTypes;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.Services;

public class VehicleEngineCompatibilityServiceTests : InMemoryDbTestBase
{
    private readonly VehicleEngineCompatibilityService _service;

    public VehicleEngineCompatibilityServiceTests()
    {
        _service = new VehicleEngineCompatibilityService(Context);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, true)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, true)]
    [InlineData(EnergyType.Ethanol, true)]
    [InlineData(EnergyType.Biofuel, true)]
    [InlineData(EnergyType.Electric, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void ValidateEnergyTypeAssignment_ValidFuelVehicleWithNoExistingEnergyType_ShouldReturnSuccess(EnergyType energyType, bool expectedResult)
    {
        // Arrange
        var vehicle = FuelVehicle;

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, energyType);

        // Assert
        result.IsSuccess.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, true)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, true)]
    [InlineData(EnergyType.Ethanol, true)]
    [InlineData(EnergyType.Biofuel, true)]
    [InlineData(EnergyType.Electric, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void ValidateEnergyTypeAssignment_ValidHybridVehicleWithNoExistingEnergyType_ShouldReturnSuccess(EnergyType energyType, bool expectedResult)
    {
        // Arrange
        var vehicle = HybridVehicle;

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, energyType);

        // Assert
        result.IsSuccess.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, true)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, true)]
    [InlineData(EnergyType.Ethanol, true)]
    [InlineData(EnergyType.Biofuel, true)]
    [InlineData(EnergyType.Electric, true)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void ValidateEnergyTypeAssignment_ValidPHEVVehicleWithNoExistingEnergyType_ShouldReturnSuccess(EnergyType energyType, bool expectedResult)
    {
        // Arrange
        var vehicle = PHEVVehicle;

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, energyType);

        // Assert
        result.IsSuccess.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, false)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Electric, true)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void ValidateEnergyTypeAssignment_ValidElectricVehicleWithNoExistingEnergyType_ShouldReturnSuccess(EnergyType energyType, bool expectedResult)
    {
        // Arrange
        var vehicle = ElectricVehicle;

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, energyType);
        
        // Assert
        result.IsSuccess.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, false)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Electric, false)]
    [InlineData(EnergyType.Hydrogen, true)]
    public void ValidateEnergyTypeAssignment_ValidHydrogenVehicleWithNoExistingEnergyType_ShouldReturnSuccess(EnergyType energyType, bool expectedResult)
    {
        // Arrange
        var vehicle = HydrogenVehicle;

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, energyType);

        // Assert
        result.IsSuccess.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline)]
    [InlineData(EnergyType.Diesel)]
    [InlineData(EnergyType.LPG)]
    [InlineData(EnergyType.CNG)]
    [InlineData(EnergyType.Ethanol)]
    [InlineData(EnergyType.Biofuel)]
    [InlineData(EnergyType.Electric)]
    public void ValidateEnergyTypeAssignment_ValidVehicleWithExistingEnergyType_ShouldReturnFailure(EnergyType energyType)
    {
        // Arrange
        var vehicle = PHEVVehicle;
        vehicle.VehicleEnergyTypes = new List<VehicleEnergyType> { new() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = energyType } };

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, energyType);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.AlreadyExists(vehicle.Id, energyType));
    }
    
    [Fact]
    public void ValidateEnergyTypeAssignment_InValidVehicleWithExistingEnergyType_ShouldReturnTwoErrors()
    {
        // Arrange
        var vehicle = ElectricVehicle;
        vehicle.VehicleEnergyTypes = new List<VehicleEnergyType> { new() { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = EnergyType.Gasoline } };

        // Act
        var result = _service.ValidateEnergyTypeAssignment(vehicle, EnergyType.Gasoline);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Validation.General");
        result.Error.Description.ShouldBe("One or more validation errors occurred");
        var validationError = result.Error as ValidationError;
        validationError.ShouldNotBeNull();
        validationError.Errors.Length.ShouldBe(2);
    }

    [Fact]
    public void ValidateEngineCompatibility_IncompatibleEnergyType_ShouldReturnFailure()
    {
        // Arrange
        var vehicle = FuelVehicle;
        var incompatibleEnergyType = EnergyType.Electric;

        // Act
        var result = _service.ValidateEngineCompatibility(vehicle.EngineType, incompatibleEnergyType);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.IncompatibleWithEngine(incompatibleEnergyType, vehicle.EngineType));
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

    // Tests for IsEnergyTypeCompatibleWithEngine method

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, true)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, true)]
    [InlineData(EnergyType.Ethanol, true)]
    [InlineData(EnergyType.Biofuel, true)]
    [InlineData(EnergyType.Electric, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void IsEnergyTypeCompatibleWithEngine_FuelEngine_ReturnsExpectedCompatibility(EnergyType energyType, bool expectedResult)
    {
        // Act
        var result = _service.IsEnergyTypeCompatibleWithEngine(energyType, EngineType.Fuel);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, true)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, true)]
    [InlineData(EnergyType.Ethanol, true)]
    [InlineData(EnergyType.Biofuel, true)]
    [InlineData(EnergyType.Electric, false)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void IsEnergyTypeCompatibleWithEngine_HybridEngine_ReturnsExpectedCompatibility(EnergyType energyType, bool expectedResult)
    {
        // Act
        var result = _service.IsEnergyTypeCompatibleWithEngine(energyType, EngineType.Hybrid);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, true)]
    [InlineData(EnergyType.Diesel, true)]
    [InlineData(EnergyType.LPG, true)]
    [InlineData(EnergyType.CNG, true)]
    [InlineData(EnergyType.Ethanol, true)]
    [InlineData(EnergyType.Biofuel, true)]
    [InlineData(EnergyType.Electric, true)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void IsEnergyTypeCompatibleWithEngine_PlugInHybridEngine_ReturnsExpectedCompatibility(EnergyType energyType, bool expectedResult)
    {
        // Act
        var result = _service.IsEnergyTypeCompatibleWithEngine(energyType, EngineType.PlugInHybrid);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, false)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Electric, true)]
    [InlineData(EnergyType.Hydrogen, false)]
    public void IsEnergyTypeCompatibleWithEngine_ElectricEngine_ReturnsExpectedCompatibility(EnergyType energyType, bool expectedResult)
    {
        // Act
        var result = _service.IsEnergyTypeCompatibleWithEngine(energyType, EngineType.Electric);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, false)]
    [InlineData(EnergyType.Diesel, false)]
    [InlineData(EnergyType.LPG, false)]
    [InlineData(EnergyType.CNG, false)]
    [InlineData(EnergyType.Ethanol, false)]
    [InlineData(EnergyType.Biofuel, false)]
    [InlineData(EnergyType.Electric, false)]
    [InlineData(EnergyType.Hydrogen, true)]
    public void IsEnergyTypeCompatibleWithEngine_HydrogenEngine_ReturnsExpectedCompatibility(EnergyType energyType, bool expectedResult)
    {
        // Act
        var result = _service.IsEnergyTypeCompatibleWithEngine(energyType, EngineType.Hydrogen);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void IsEnergyTypeCompatibleWithEngine_UnknownEngineType_ReturnsFalse()
    {
        // Act
        var result = _service.IsEnergyTypeCompatibleWithEngine(EnergyType.Gasoline, (EngineType)999);

        // Assert
        result.ShouldBeFalse();
    }
    
    [Fact]
    public async Task GetCompatibleEnergyTypesForEngine_FuelEngineType_ReturnsExpectedEnergyTypes()
    {
        // Act
        var result = await _service.GetCompatibleEnergyTypesForEngine(EngineType.Fuel);

        // Assert
        EnergyType[] expectedEnergyTypes =
        [
            EnergyType.Gasoline,
            EnergyType.Diesel,
            EnergyType.LPG,
            EnergyType.CNG,
            EnergyType.Ethanol,
            EnergyType.Biofuel
        ];
        
        result.ShouldBeEquivalentTo(expectedEnergyTypes);
    }
    
    [Fact]
    public async Task GetCompatibleEnergyTypesForEngine_HybridEngineType_ReturnsExpectedEnergyTypes()
    {
        // Act
        var result = await _service.GetCompatibleEnergyTypesForEngine(EngineType.Hybrid);

        // Assert
        EnergyType[] expectedEnergyTypes =
        [
            EnergyType.Gasoline,
            EnergyType.Diesel,
            EnergyType.LPG,
            EnergyType.CNG,
            EnergyType.Ethanol,
            EnergyType.Biofuel,
        ];
        
        result.ShouldBeEquivalentTo(expectedEnergyTypes);
    }
    
    [Fact]
    public async Task GetCompatibleEnergyTypesForEngine_PhevEngineType_ReturnsExpectedEnergyTypes()
    {
        // Act
        var result = await _service.GetCompatibleEnergyTypesForEngine(EngineType.PlugInHybrid);

        // Assert
        EnergyType[] expectedEnergyTypes =
        [
            EnergyType.Gasoline,
            EnergyType.Diesel,
            EnergyType.LPG,
            EnergyType.CNG,
            EnergyType.Ethanol,
            EnergyType.Biofuel,
            EnergyType.Electric
        ];
        
        result.ShouldBeEquivalentTo(expectedEnergyTypes);
    }

    [Fact]
    public async Task GetCompatibleEnergyTypesForEngine_ElectricEngineType_ReturnsExpectedEnergyTypes()
    {
        // Act
        var result = await _service.GetCompatibleEnergyTypesForEngine(EngineType.Electric);

        // Assert
        EnergyType[] expectedEnergyTypes =
        [
            EnergyType.Electric
        ];
        
        result.ShouldBeEquivalentTo(expectedEnergyTypes);
    }
    
    [Fact]
    public async Task GetCompatibleEnergyTypesForEngine_HydrogenEngineType_ReturnsExpectedEnergyTypes()
    {
        // Act
        var result = await _service.GetCompatibleEnergyTypesForEngine(EngineType.Hydrogen);

        // Assert
        EnergyType[] expectedEnergyTypes =
        [
            EnergyType.Hydrogen
        ];
        
        result.ShouldBeEquivalentTo(expectedEnergyTypes);
    }
    
    private async Task<Vehicle> CreateGasolineVehicleInDb()
    {
        var vehicle = FuelVehicle;
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


    private static Vehicle FuelVehicle => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Audi",
        Model = "Test Car",
        EngineType = EngineType.Fuel,
        UserId = Guid.NewGuid(),
    };

    private static Vehicle HybridVehicle => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Toyota",
        Model = "Test Car",
        EngineType = EngineType.Hybrid,
        UserId = Guid.NewGuid(),
    };

    private static Vehicle PHEVVehicle => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Toyota",
        Model = "Test Car",
        EngineType = EngineType.PlugInHybrid,
        UserId = Guid.NewGuid(),
    };

    private static Vehicle ElectricVehicle => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Tesla",
        Model = "Test Car",
        EngineType = EngineType.Electric,
        UserId = Guid.NewGuid(),
    };

    private static Vehicle HydrogenVehicle => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Toyota",
        Model = "Test Car",
        EngineType = EngineType.Hydrogen,
        UserId = Guid.NewGuid(),
    };
}