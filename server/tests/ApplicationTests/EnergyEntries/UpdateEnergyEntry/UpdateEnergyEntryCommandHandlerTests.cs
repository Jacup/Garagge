using Application.Abstractions.Services;
using Application.EnergyEntries;
using Application.EnergyEntries.Update;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.EnergyEntries;

public class UpdateEnergyEntryCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateEnergyEntryCommandHandler _handler;
    private readonly Mock<IVehicleEngineCompatibilityService> _energyCompatibilityServiceMock;
    private readonly Mock<IEnergyEntryMileageValidator> _mileageValidatorMock;

    public UpdateEnergyEntryCommandHandlerTests()
    {
        _energyCompatibilityServiceMock = new Mock<IVehicleEngineCompatibilityService>();
        _mileageValidatorMock = new Mock<IEnergyEntryMileageValidator>();
        _handler = new UpdateEnergyEntryCommandHandler(Context, UserContextMock.Object, _energyCompatibilityServiceMock.Object, _mileageValidatorMock.Object);

        // Setup default behavior
        _energyCompatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleAsync(It.IsAny<Guid>(), It.IsAny<EnergyType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mileageValidatorMock
            .Setup(x => x.IsValid(It.IsAny<IReadOnlyCollection<EnergyEntry>>(), It.IsAny<EnergyEntry>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
            .Returns(true);
    }

    private async Task<Vehicle> CreateVehicleInDb(EngineType engineType = EngineType.Fuel, Guid? userId = null)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? AuthorizedUserId,
            Brand = "TestBrand",
            Model = "TestModel",
            EngineType = engineType
        };
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private async Task<EnergyEntry> CreateEnergyEntryInDb(Vehicle vehicle, DateOnly? date = null, int? mileage = null)
    {
        var entry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Vehicle = null!,
            Date = date ?? new DateOnly(2024, 1, 1),
            Mileage = mileage ?? 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 10
        };
        Context.EnergyEntries.Add(entry);
        await Context.SaveChangesAsync();
        return entry;
    }

    private static UpdateEnergyEntryCommand CreateValidCommand(Guid vehicleId, Guid entryId) =>
        new(vehicleId, entryId, new DateOnly(2024, 2, 2), 2000, EnergyType.Gasoline, EnergyUnit.Liter, 20, 200, 10);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithEnergyEntryDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(entry.Id);
        result.Value.Date.ShouldBe(command.Date);
        result.Value.Mileage.ShouldBe(command.Mileage);
        result.Value.Type.ShouldBe(command.Type);
        result.Value.EnergyUnit.ShouldBe(command.EnergyUnit);
        result.Value.Volume.ShouldBe(command.Volume);
        result.Value.Cost.ShouldBe(command.Cost);
        result.Value.PricePerUnit.ShouldBe(command.PricePerUnit);
        result.Value.VehicleId.ShouldBe(command.VehicleId);

        var updatedEntry = Context.EnergyEntries.FirstOrDefault(ee => ee.Id == entry.Id);
        updatedEntry.ShouldNotBeNull();
        updatedEntry.Date.ShouldBe(command.Date);
        updatedEntry.Mileage.ShouldBe(command.Mileage);
        updatedEntry.Type.ShouldBe(command.Type);
        updatedEntry.EnergyUnit.ShouldBe(command.EnergyUnit);
        updatedEntry.Volume.ShouldBe(command.Volume);
        updatedEntry.Cost.ShouldBe(command.Cost);
        updatedEntry.PricePerUnit.ShouldBe(command.PricePerUnit);
    }

    [Fact]
    public async Task Handle_EntryNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var nonExistentEntryId = Guid.NewGuid();
        var command = CreateValidCommand(vehicle.Id, nonExistentEntryId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleIdMismatch_ReturnsFailureWithInvalidError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle);
        var otherVehicleId = Guid.NewGuid();
        var command = CreateValidCommand(otherVehicleId, entry.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb(EngineType.Fuel, otherUserId);
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_IncompatibleEnergyType_ReturnsFailureWithIncompatibleEnergyTypeError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id) with { Type = EnergyType.Electric };

        _energyCompatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.TypeIncompatible);
    }

    [Fact]
    public async Task Handle_IncorrectMileage_ReturnsFailureWithIncorrectMileageError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id);

        _mileageValidatorMock
            .Setup(x => x.IsValid(It.IsAny<IReadOnlyCollection<EnergyEntry>>(), It.IsAny<EnergyEntry>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.MileageIncorrect);
    }

    [Fact]
    public async Task Handle_MileageValidatorCalledWithCorrectParameters_VerifiesCall()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mileageValidatorMock.Verify(
            x => x.IsValid(
                It.Is<IReadOnlyCollection<EnergyEntry>>(entries => entries.Any(e => e.Id == entry.Id)),
                It.Is<EnergyEntry>(e => e.Id == entry.Id),
                command.Date,
                command.Mileage),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EnergyCompatibilityServiceCalledWithCorrectParameters_VerifiesCall()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _energyCompatibilityServiceMock.Verify(
            x => x.IsEnergyTypeCompatibleAsync(vehicle.Id, command.Type, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateWithDifferentEnergyType_ReturnsSuccessWhenCompatible()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EngineType.Hybrid);
        var entry = await CreateEnergyEntryInDb(vehicle);
        var command = CreateValidCommand(vehicle.Id, entry.Id) with { Type = EnergyType.Electric, EnergyUnit = EnergyUnit.kWh };

        _energyCompatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleAsync(vehicle.Id, EnergyType.Electric, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(EnergyType.Electric);
        result.Value.EnergyUnit.ShouldBe(EnergyUnit.kWh);
    }

    [Fact]
    public async Task Handle_UpdateWithSameMileageAndDate_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb();
        var entry = await CreateEnergyEntryInDb(vehicle, new DateOnly(2024, 1, 15), 1500);
        var command = CreateValidCommand(vehicle.Id, entry.Id) with { Date = entry.Date, Mileage = entry.Mileage, Volume = 25 };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Date.ShouldBe(entry.Date);
        result.Value.Mileage.ShouldBe(entry.Mileage);
        result.Value.Volume.ShouldBe(25);
    }
}