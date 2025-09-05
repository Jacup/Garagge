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

    #region IsEnergyTypeCompatibleAsync Tests

    [Fact]
    public async Task IsEnergyTypeCompatibleAsync_WhenVehicleSupportsEnergyType_ReturnsTrue()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Electric);

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Gasoline);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsEnergyTypeCompatibleAsync_WhenVehicleDoesNotSupportEnergyType_ReturnsFalse()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsEnergyTypeCompatibleAsync_WhenVehicleNotFound_ReturnsFalse()
    {
        // Arrange
        var nonExistentVehicleId = Guid.NewGuid();

        // Act
        var result = await _service.IsEnergyTypeCompatibleAsync(nonExistentVehicleId, EnergyType.Gasoline);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsEnergyTypeCompatibleAsync_WhenVehicleSupportsMultipleTypes_ReturnsCorrectResults()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Diesel | EnergyType.Electric);

        // Act & Assert
        var gasolineResult = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Gasoline);
        var dieselResult = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Diesel);
        var electricResult = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric);
        var lpgResult = await _service.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.LPG);

        gasolineResult.ShouldBeTrue();
        dieselResult.ShouldBeTrue();
        electricResult.ShouldBeTrue();
        lpgResult.ShouldBeFalse();
    }

    #endregion

    #region GetCompatibleEnergyTypesAsync Tests

    [Fact]
    public async Task GetCompatibleEnergyTypesAsync_WhenVehicleExists_ReturnsCorrectTypes()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Electric);

        // Act
        var result = await _service.GetCompatibleEnergyTypesAsync(vehicle.Id);

        // Assert
        var compatibleTypes = result.ToList();
        compatibleTypes.Count.ShouldBe(2);
        compatibleTypes.ShouldContain(EnergyType.Gasoline);
        compatibleTypes.ShouldContain(EnergyType.Electric);
    }

    [Fact]
    public async Task GetCompatibleEnergyTypesAsync_WhenVehicleNotFound_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentVehicleId = Guid.NewGuid();

        // Act
        var result = await _service.GetCompatibleEnergyTypesAsync(nonExistentVehicleId);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCompatibleEnergyTypesAsync_WhenVehicleSupportsAllFuels_ReturnsAllFuelTypes()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.AllFuels);

        // Act
        var result = await _service.GetCompatibleEnergyTypesAsync(vehicle.Id);

        // Assert
        var compatibleTypes = result.ToList();
        compatibleTypes.ShouldContain(EnergyType.Gasoline);
        compatibleTypes.ShouldContain(EnergyType.Diesel);
        compatibleTypes.ShouldContain(EnergyType.LPG);
        compatibleTypes.ShouldContain(EnergyType.CNG);
        compatibleTypes.ShouldContain(EnergyType.Ethanol);
        compatibleTypes.ShouldContain(EnergyType.Biofuel);
        compatibleTypes.ShouldContain(EnergyType.Hydrogen);
        compatibleTypes.ShouldNotContain(EnergyType.Electric);
    }

    [Fact]
    public async Task GetCompatibleEnergyTypesAsync_WhenVehicleSupportsElectricOnly_ReturnsOnlyElectric()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Electric);

        // Act
        var result = await _service.GetCompatibleEnergyTypesAsync(vehicle.Id);

        // Assert
        var compatibleTypes = result.ToList();
        compatibleTypes.Count.ShouldBe(1);
        compatibleTypes.ShouldContain(EnergyType.Electric);
    }

    #endregion

    #region IsAnyEnergyTypeCompatibleAsync Tests

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenEnergyTypeIsNone_ReturnsFalse()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenSingleTypeMatches_ReturnsTrue()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Diesel);

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Gasoline);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenMultipleTypesOneMatches_ReturnsTrue()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Diesel);

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Gasoline | EnergyType.Electric);

        // Assert
        result.ShouldBeTrue(); // Gasoline matches
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenNoTypesMatch_ReturnsFalse()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Diesel);

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric | EnergyType.LPG);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenAllFuelsRequested_ReturnsCorrectResult()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Electric);

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.AllFuels);

        // Assert
        result.ShouldBeTrue(); // Gasoline is part of AllFuels
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenAllRequestedAndVehicleSupportsElectricOnly_ReturnsFalse()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Electric);

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.AllFuels);

        // Assert
        result.ShouldBeFalse(); // Electric is not part of AllFuels
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WhenVehicleNotFound_ReturnsFalse()
    {
        // Arrange
        var nonExistentVehicleId = Guid.NewGuid();

        // Act
        var result = await _service.IsAnyEnergyTypeCompatibleAsync(nonExistentVehicleId, EnergyType.Gasoline);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task IsAnyEnergyTypeCompatibleAsync_WithComplexFlags_ReturnsCorrectResult()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb(EnergyType.Diesel | EnergyType.LPG | EnergyType.Electric);

        // Act & Assert
        // Test various flag combinations
        var result1 = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Gasoline | EnergyType.Diesel);
        result1.ShouldBeTrue(); // Diesel matches

        var result2 = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Gasoline | EnergyType.CNG);
        result2.ShouldBeFalse(); // Neither matches

        var result3 = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric | EnergyType.Hydrogen);
        result3.ShouldBeTrue(); // Electric matches

        var result4 = await _service.IsAnyEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.All);
        result4.ShouldBeTrue(); // Vehicle supports some types that are part of All
    }

    #endregion

    #region Integration Tests with Multiple Vehicles

    [Fact]
    public async Task MultipleVehicles_DifferentCompatibilities_WorkCorrectly()
    {
        // Arrange
        var gasolineVehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var electricVehicle = await CreateVehicleInDb(EnergyType.Electric);
        var hybridVehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Electric);

        // Act & Assert
        // Gasoline vehicle
        (await _service.IsEnergyTypeCompatibleAsync(gasolineVehicle.Id, EnergyType.Gasoline)).ShouldBeTrue();
        (await _service.IsEnergyTypeCompatibleAsync(gasolineVehicle.Id, EnergyType.Electric)).ShouldBeFalse();

        // Electric vehicle
        (await _service.IsEnergyTypeCompatibleAsync(electricVehicle.Id, EnergyType.Electric)).ShouldBeTrue();
        (await _service.IsEnergyTypeCompatibleAsync(electricVehicle.Id, EnergyType.Gasoline)).ShouldBeFalse();

        // Hybrid vehicle
        (await _service.IsEnergyTypeCompatibleAsync(hybridVehicle.Id, EnergyType.Gasoline)).ShouldBeTrue();
        (await _service.IsEnergyTypeCompatibleAsync(hybridVehicle.Id, EnergyType.Electric)).ShouldBeTrue();
        (await _service.IsEnergyTypeCompatibleAsync(hybridVehicle.Id, EnergyType.Diesel)).ShouldBeFalse();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetCompatibleEnergyTypesAsync_WithManyVehicleEnergyTypes_PerformsWell()
    {
        // Arrange - Create vehicle with all possible energy types
        var vehicle = await CreateVehicleInDb(EnergyType.All);

        // Act
        var startTime = DateTime.UtcNow;
        var result = await _service.GetCompatibleEnergyTypesAsync(vehicle.Id);
        var endTime = DateTime.UtcNow;

        // Assert
        var compatibleTypes = result.ToList();
        compatibleTypes.ShouldNotBeEmpty();
        (endTime - startTime).TotalMilliseconds.ShouldBeLessThan(100); // Should complete quickly
    }

    #endregion

    #region Helper Methods

    private async Task<Vehicle> CreateVehicleInDb(EnergyType supportedEnergyTypes)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Test Car",
            PowerType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        // Define basic energy types (individual flags, not combinations)
        var basicEnergyTypes = new[]
        {
            EnergyType.Gasoline,
            EnergyType.Diesel,
            EnergyType.LPG,
            EnergyType.CNG,
            EnergyType.Ethanol,
            EnergyType.Biofuel,
            EnergyType.Hydrogen,
            EnergyType.Electric
        };

        // Add supported energy types based on flags
        foreach (var energyType in basicEnergyTypes)
        {
            if ((supportedEnergyTypes & energyType) != 0)
            {
                vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicle.Id,
                    EnergyType = energyType
                });
            }
        }

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    #endregion
}
