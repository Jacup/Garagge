using Application.Core;
using Application.Services;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace ApplicationTests.Services;

public class VehicleUpdateValidationServiceTests : InMemoryDbTestBase
{
    private readonly VehicleUpdateValidationService _sut;

    public VehicleUpdateValidationServiceTests()
    {
        _sut = new VehicleUpdateValidationService(Context);
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_OnlyNewEnergyTypes_ReturnsSuccess()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid()
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Gasoline,
            EnergyType.Electric
        };
        
        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle, 
            requestedEnergyTypes, 
            CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldBe(requestedEnergyTypes);
        result.Value.TypesToRemove.ShouldBeEmpty();
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeTrue();
    }
    
    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_AddAndRemoveEnergyType_ReturnsSuccess()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Diesel,
                    VehicleId = vehicleId
                }
            }
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Gasoline,
        };
        
        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle, 
            requestedEnergyTypes, 
            CancellationToken.None);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldBe(requestedEnergyTypes);
        result.Value.TypesToRemove.ShouldBe(vehicle.AllowedEnergyTypes);
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_NoChanges_ReturnsSuccessWithNoChanges()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "BMW",
            Model = "X5",
            EngineType = EngineType.Hybrid,
            ManufacturedYear = 2022,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Gasoline,
                    VehicleId = vehicleId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Electric,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Gasoline,
            EnergyType.Electric
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldBeEmpty();
        result.Value.TypesToRemove.ShouldBeEmpty();
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_RemoveEnergyTypeWithExistingEntries_ReturnsFailure()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2019,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Diesel,
                    VehicleId = vehicleId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.LPG,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);

        var existingEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                Vehicle = null!,
                Type = EnergyType.LPG,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Mileage = 50000,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 40
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                Vehicle = null!,
                Type = EnergyType.LPG,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                Mileage = 49500,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 35
            }
        };

        Context.EnergyEntries.AddRange(existingEntries);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Diesel
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(VehicleErrors.CannotRemoveEnergyTypes([], 0).Code);
        result.Error.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_RemoveMultipleEnergyTypesWithExistingEntries_ReturnsFailureWithAllConflictingTypes()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Mercedes",
            Model = "C-Class",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2021,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Gasoline,
                    VehicleId = vehicleId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Diesel,
                    VehicleId = vehicleId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.LPG,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);

        var existingEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                Vehicle = null!,
                Type = EnergyType.Diesel,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Mileage = 50000,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 50
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                Vehicle = null!,
                Type = EnergyType.LPG,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                Mileage = 49000,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 40
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                Vehicle = null!,
                Type = EnergyType.Diesel,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)),
                Mileage = 48500,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 45
            }
        };

        Context.EnergyEntries.AddRange(existingEntries);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Gasoline
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(VehicleErrors.CannotRemoveEnergyTypes([], 0).Code);
        result.Error.Type.ShouldBe(ErrorType.Conflict);
        result.Error.Description.ShouldContain("Diesel");
        result.Error.Description.ShouldContain("LPG");
        result.Error.Description.ShouldContain("3");
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_RemoveEnergyTypeWithoutExistingEntries_ReturnsSuccess()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Ford",
            Model = "Focus",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2018,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Gasoline,
                    VehicleId = vehicleId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.LPG,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Gasoline
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldBeEmpty();
        result.Value.TypesToRemove.ShouldContain(EnergyType.LPG);
        result.Value.TypesToRemove.Count.ShouldBe(1);
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_AddMultipleEnergyTypes_ReturnsSuccess()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Tesla",
            Model = "Model 3",
            EngineType = EngineType.Electric,
            ManufacturedYear = 2023,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Electric,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Electric,
            EnergyType.Gasoline,
            EnergyType.Diesel
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldContain(EnergyType.Gasoline);
        result.Value.TypesToAdd.ShouldContain(EnergyType.Diesel);
        result.Value.TypesToAdd.Count.ShouldBe(2);
        result.Value.TypesToRemove.ShouldBeEmpty();
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_EmptyRequestedTypes_ReturnsSuccessWithAllTypesRemoved()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Volkswagen",
            Model = "Golf",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Gasoline,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>();

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldBeEmpty();
        result.Value.TypesToRemove.ShouldContain(EnergyType.Gasoline);
        result.Value.TypesToRemove.Count.ShouldBe(1);
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_VehicleWithNoEnergyTypes_ReturnsSuccessWithTypesToAdd()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Skoda",
            Model = "Octavia",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2021,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Diesel,
            EnergyType.CNG
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TypesToAdd.ShouldBe(requestedEnergyTypes);
        result.Value.TypesToRemove.ShouldBeEmpty();
        result.Value.ConflictingEnergyTypes.ShouldBeEmpty();
        result.Value.HasChanges.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEnergyTypesChangeAsync_PartialRemoveWithConflicts_ReturnsFailure()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Honda",
            Model = "Civic",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2019,
            Type = VehicleType.Car,
            UserId = Guid.NewGuid(),
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.Gasoline,
                    VehicleId = vehicleId
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    EnergyType = EnergyType.CNG,
                    VehicleId = vehicleId
                }
            }
        };

        Context.Vehicles.Add(vehicle);

        var existingEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                Vehicle = null!,
                Type = EnergyType.CNG,
                Date = DateOnly.FromDateTime(DateTime.Now),
                Mileage = 30000,
                EnergyUnit = EnergyUnit.kWh,
                Volume = 10
            }
        };

        Context.EnergyEntries.AddRange(existingEntries);
        await Context.SaveChangesAsync();

        var requestedEnergyTypes = new List<EnergyType>
        {
            EnergyType.Gasoline,
            EnergyType.Diesel
        };

        // Act
        var result = await _sut.ValidateEnergyTypesChangeAsync(
            vehicle,
            requestedEnergyTypes,
            CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(VehicleErrors.CannotRemoveEnergyTypes([], 0).Code);
        result.Error.Type.ShouldBe(ErrorType.Conflict);
        result.Error.Description.ShouldContain("CNG");
        result.Error.Description.ShouldContain("1");
    }
}