using Application.Abstractions;
using Application.EnergyEntries;
using Application.EnergyEntries.CreateChargingEntry;
using Application.EnergyEntries.Dtos;
using Application.Vehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.EnergyEntries.CreateChargingEntry;

public class CreateChargingEntryCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IVehicleEnergyValidator> _vehicleEnergyValidatorMock;
    private readonly CreateChargingEntryCommandHandler _handler;

    public CreateChargingEntryCommandHandlerTests()
    {
        _vehicleEnergyValidatorMock = new Mock<IVehicleEnergyValidator>();
        _handler = new CreateChargingEntryCommandHandler(
            Context, 
            UserContextMock.Object, 
            _vehicleEnergyValidatorMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithChargingEntryDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(PowerType.Electric);
        var command = CreateValidCommand(vehicle.Id);

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeCharged(PowerType.Electric)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Date.ShouldBe(command.Date);
        result.Value.Mileage.ShouldBe(command.Mileage);
        result.Value.Cost.ShouldBe(command.Cost);
        result.Value.EnergyAmount.ShouldBe(command.EnergyAmount);
        result.Value.Unit.ShouldBe(command.Unit);
        result.Value.PricePerUnit.ShouldBe(command.PricePerUnit);
        result.Value.ChargingDurationMinutes.ShouldBe(command.ChargingDurationMinutes);
        result.Value.VehicleId.ShouldBe(command.VehicleId);

        var savedChargingEntry = Context.ChargingEntries.FirstOrDefault(ce => ce.VehicleId == vehicle.Id);
        savedChargingEntry.ShouldNotBeNull();
        savedChargingEntry.Date.ShouldBe(command.Date);
        savedChargingEntry.Mileage.ShouldBe(command.Mileage);
        savedChargingEntry.EnergyAmount.ShouldBe(command.EnergyAmount);
        savedChargingEntry.ChargingDurationMinutes.ShouldBe(command.ChargingDurationMinutes);
    }

    [Fact]
    public async Task Handle_ValidCommandWithNullChargingDuration_ReturnsSuccessWithChargingEntryDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(PowerType.Electric);
        var command = CreateValidCommand(vehicle.Id) with { ChargingDurationMinutes = null };

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeCharged(PowerType.Electric)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ChargingDurationMinutes.ShouldBeNull();

        var savedChargingEntry = Context.ChargingEntries.FirstOrDefault(ce => ce.VehicleId == vehicle.Id);
        savedChargingEntry.ShouldNotBeNull();
        savedChargingEntry.ChargingDurationMinutes.ShouldBeNull();
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
        result.Error.ShouldBe(VehicleErrors.NotFound(nonExistentVehicleId));

        Context.ChargingEntries.Count().ShouldBe(0);
    }

    [Fact]
    public async Task UserId_IsNotVehicleOwner_ReturnsUnauthorizedError()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        UserContextMock.Setup(x => x.UserId).Returns(differentUserId);
        
        var vehicle = await CreateVehicleInDb(PowerType.Electric);
        var command = CreateValidCommand(vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(FuelEntryErrors.Unauthorized);

        Context.ChargingEntries.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData(PowerType.Gasoline)]
    [InlineData(PowerType.Diesel)]
    public async Task PowerType_CannotBeCharged_ReturnsIncompatiblePowerTypeError(PowerType powerType)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(powerType);
        var command = CreateValidCommand(vehicle.Id);

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeCharged(powerType)).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(FuelEntryErrors.IncompatiblePowerType(vehicle.Id, powerType));

        Context.ChargingEntries.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData(PowerType.Electric)]
    [InlineData(PowerType.PlugInHybrid)]
    public async Task PowerType_CanBeCharged_ReturnsSuccess(PowerType powerType)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(powerType);
        var command = CreateValidCommand(vehicle.Id);

        _vehicleEnergyValidatorMock.Setup(x => x.CanBeCharged(powerType)).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        Context.ChargingEntries.Count().ShouldBe(1);
    }

    private static CreateChargingEntryCommand CreateValidCommand(Guid vehicleId)
    {
        return new CreateChargingEntryCommand(
            vehicleId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            50000,
            85.30m,
            42.5m,
            EnergyUnit.kWh,
            2.01m,
            90);
    }

    private async Task<Vehicle> CreateVehicleInDb(PowerType powerType)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Tesla",
            Model = "Model 3",
            PowerType = powerType,
            UserId = LoggedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }
}