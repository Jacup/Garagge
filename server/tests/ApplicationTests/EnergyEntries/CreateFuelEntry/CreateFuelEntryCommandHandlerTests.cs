using Application.Abstractions;
using Application.EnergyEntries;
using Application.EnergyEntries.CreateFuelEntry;
using Application.Vehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.EnergyEntries.CreateFuelEntry;

public class CreateFuelEntryCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IVehicleEnergyValidator> _vehicleEnergyValidatorMock;
    private readonly CreateFuelEntryCommandHandler _handler;

    public CreateFuelEntryCommandHandlerTests()
    {
        _vehicleEnergyValidatorMock = new Mock<IVehicleEnergyValidator>();
        _handler = new CreateFuelEntryCommandHandler(
            Context, 
            UserContextMock.Object, 
            _vehicleEnergyValidatorMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithFuelEntryDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(PowerType.Gasoline);
        var command = CreateValidCommand(vehicle.Id);

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeFueled(PowerType.Gasoline)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Date.ShouldBe(command.Date);
        result.Value.Mileage.ShouldBe(command.Mileage);
        result.Value.Cost.ShouldBe(command.Cost);
        result.Value.Volume.ShouldBe(command.Volume);
        result.Value.Unit.ShouldBe(command.Unit);
        result.Value.PricePerUnit.ShouldBe(command.PricePerUnit);
        result.Value.VehicleId.ShouldBe(command.VehicleId);

        var savedFuelEntry = Context.FuelEntries.FirstOrDefault(fe => fe.VehicleId == vehicle.Id);
        savedFuelEntry.ShouldNotBeNull();
        savedFuelEntry.Date.ShouldBe(command.Date);
        savedFuelEntry.Mileage.ShouldBe(command.Mileage);
    }

    [Fact]
    public async Task VehicleId_DoesNotExist_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var command = CreateValidCommand(nonExistentVehicleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound(nonExistentVehicleId));

        Context.FuelEntries.Count().ShouldBe(0);
    }

    [Fact]
    public async Task UserId_IsNotVehicleOwner_ReturnsUnauthorizedError()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        UserContextMock.Setup(x => x.UserId).Returns(differentUserId);
        
        var vehicle = await CreateVehicleInDb(PowerType.Gasoline);
        var command = CreateValidCommand(vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.Unauthorized);

        Context.FuelEntries.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData(PowerType.Electric)]
    [InlineData(PowerType.PlugInHybrid)]
    public async Task PowerType_CannotBeFueled_ReturnsIncompatiblePowerTypeError(PowerType powerType)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(powerType);
        var command = CreateValidCommand(vehicle.Id);

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeFueled(powerType)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.IncompatiblePowerType(vehicle.Id, powerType));

        Context.FuelEntries.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData(PowerType.Gasoline)]
    [InlineData(PowerType.Diesel)]
    [InlineData(PowerType.Hybrid)]
    public async Task PowerType_CanBeFueled_ReturnsSuccess(PowerType powerType)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(powerType);
        var command = CreateValidCommand(vehicle.Id);

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeFueled(powerType)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        Context.FuelEntries.Count().ShouldBe(1);
    }

    private static CreateFuelEntryCommand CreateValidCommand(Guid vehicleId)
    {
        return new CreateFuelEntryCommand(
            vehicleId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            10000,
            50.00m,
            40.50m,
            VolumeUnit.Liters,
            1.25m);
    }

    private async Task<Vehicle> CreateVehicleInDb(PowerType powerType)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "TestBrand",
            Model = "TestModel",
            PowerType = powerType,
            UserId = LoggedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }
}
