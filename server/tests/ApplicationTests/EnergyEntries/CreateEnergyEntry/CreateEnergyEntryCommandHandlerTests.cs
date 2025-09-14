using Application.Abstractions.Services;
using Application.Core;
using Application.EnergyEntries;
using Application.EnergyEntries.Create;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.EnergyEntries.CreateEnergyEntry;

public class CreateEnergyEntryCommandHandlerTests : InMemoryDbTestBase
{
    private readonly CreateEnergyEntryCommandHandler _handler;
    private readonly Mock<IVehicleEngineCompatibilityService> _energyCompatibilityServiceMock;

    public CreateEnergyEntryCommandHandlerTests()
    {
        _energyCompatibilityServiceMock = new Mock<IVehicleEngineCompatibilityService>();
        _handler = new CreateEnergyEntryCommandHandler(Context, UserContextMock.Object, _energyCompatibilityServiceMock.Object);

        // Setup default behavior for energy compatibility service
        _energyCompatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleAsync(It.IsAny<Guid>(), It.IsAny<EnergyType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithEnergyEntryDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var command = CreateValidCommand(vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Date.ShouldBe(command.Date);
        result.Value.Mileage.ShouldBe(command.Mileage);
        result.Value.Type.ShouldBe(command.Type);
        result.Value.EnergyUnit.ShouldBe(command.EnergyUnit);
        result.Value.Volume.ShouldBe(command.Volume);
        result.Value.Cost.ShouldBe(command.Cost);
        result.Value.PricePerUnit.ShouldBe(command.PricePerUnit);
        result.Value.VehicleId.ShouldBe(command.VehicleId);

        var savedEntry = Context.EnergyEntries.FirstOrDefault(ee => ee.VehicleId == vehicle.Id);
        savedEntry.ShouldNotBeNull();
        savedEntry.Date.ShouldBe(command.Date);
        savedEntry.Mileage.ShouldBe(command.Mileage);
        savedEntry.Type.ShouldBe(command.Type);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsFailureWithVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var command = CreateValidCommand(nonExistentVehicleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(nonExistentVehicleId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsFailureWithUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline, otherUserId);
        var command = CreateValidCommand(vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_IncompatibleEnergyType_ReturnsFailureWithIncompatibleEnergyTypeError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline); // Vehicle supports only Gasoline
        var command = CreateValidCommand(vehicle.Id) with { Type = EnergyType.Electric }; // Try to add Electricity

        // Setup mock to return false for incompatible energy type
        _energyCompatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.IncompatibleEnergyType(vehicle.Id, EnergyType.Electric));
    }

    [Fact]
    public async Task Handle_MileageLowerThanLastEntry_ReturnsFailureWithIncorrectMileageError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Add existing entry with higher mileage
        var existingEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Date = new DateOnly(2023, 10, 1),
            Mileage = 2000, // Higher mileage
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50m
        };
        Context.EnergyEntries.Add(existingEntry);
        await Context.SaveChangesAsync();

        var command = CreateValidCommand(vehicle.Id) with { Mileage = 1500 }; // Lower mileage

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.IncorrectMileage);
    }

    [Fact]
    public async Task Handle_MileageEqualToLastEntry_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Add existing entry
        var existingEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Date = new DateOnly(2023, 10, 1),
            Mileage = 2000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50m
        };
        Context.EnergyEntries.Add(existingEntry);
        await Context.SaveChangesAsync();

        var command = CreateValidCommand(vehicle.Id) with { Mileage = 2000 }; // Equal mileage

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBe(Error.None);
    }

    [Fact]
    public async Task Handle_MileageGreaterThanLastEntry_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Add existing entry
        var existingEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Date = new DateOnly(2023, 10, 1),
            Mileage = 2000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50m
        };
        Context.EnergyEntries.Add(existingEntry);
        await Context.SaveChangesAsync();

        var command = CreateValidCommand(vehicle.Id) with { Mileage = 2500 }; // Higher mileage

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Mileage.ShouldBe(2500);
    }

    [Fact]
    public async Task Handle_NoExistingEntries_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var command = CreateValidCommand(vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_VehicleSupportsMultipleEnergyTypes_ReturnsSuccessForAllSupportedTypes()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline, AuthorizedUserId, new[] { EnergyType.Gasoline, EnergyType.Electric });

        // Test Gasoline
        var gasolineCommand = CreateValidCommand(vehicle.Id) with { Type = EnergyType.Gasoline, Mileage = 1000 };
        var gasolineResult = await _handler.Handle(gasolineCommand, CancellationToken.None);

        // Test Electricity
        var electricityCommand = CreateValidCommand(vehicle.Id) with { Type = EnergyType.Electric, Mileage = 2000, EnergyUnit = EnergyUnit.kWh };
        var electricityResult = await _handler.Handle(electricityCommand, CancellationToken.None);

        // Assert
        gasolineResult.IsSuccess.ShouldBeTrue();
        electricityResult.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithNullOptionalFields_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var command = CreateValidCommand(vehicle.Id) with { Cost = null, PricePerUnit = null };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Cost.ShouldBeNull();
        result.Value.PricePerUnit.ShouldBeNull();
    }

    private async Task<Vehicle> CreateVehicleInDb(EnergyType energyType, Guid? userId = null, EnergyType[]? supportedEnergyTypes = null)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = userId ?? AuthorizedUserId,
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        var energyTypesToAdd = supportedEnergyTypes ?? new[] { energyType };
        foreach (var type in energyTypesToAdd)
        {
            vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = type });
        }

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private static CreateEnergyEntryCommand CreateValidCommand(Guid vehicleId)
    {
        return new CreateEnergyEntryCommand(
            VehicleId: vehicleId,
            Date: new DateOnly(2023, 10, 14),
            Mileage: 1000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.0m,
            Cost: 100.0m,
            PricePerUnit: 2.0m);
    }
}