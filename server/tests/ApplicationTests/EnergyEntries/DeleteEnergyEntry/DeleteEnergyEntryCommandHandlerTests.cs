using Application.EnergyEntries;
using Application.EnergyEntries.DeleteEnergyEntry;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace ApplicationTests.EnergyEntries.DeleteEnergyEntry;

public class DeleteEnergyEntryCommandHandlerTests : InMemoryDbTestBase
{
    private readonly DeleteEnergyEntryCommandHandler _handler;

    public DeleteEnergyEntryCommandHandlerTests()
    {
        _handler = new DeleteEnergyEntryCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidFuelEntryDeletion_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(PowerType.Gasoline);
        var fuelEntry = await CreateFuelEntryInDb(vehicle.Id);
        var command = new DeleteEnergyEntryCommand(fuelEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var deletedEntry = Context.EnergyEntries.FirstOrDefault(fe => fe.Id == fuelEntry.Id);
        deletedEntry.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ValidChargingEntryDeletion_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(PowerType.Electric);
        var chargingEntry = await CreateChargingEntryInDb(vehicle.Id);
        var command = new DeleteEnergyEntryCommand(chargingEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var deletedEntry = Context.EnergyEntries.FirstOrDefault(ce => ce.Id == chargingEntry.Id);
        deletedEntry.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsUnauthorizedError()
    {
        // Arrange
        UserContextMock.Setup(x => x.UserId).Returns(Guid.Empty);
        var command = new DeleteEnergyEntryCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb(PowerType.Gasoline, otherUserId);
        var fuelEntry = await CreateFuelEntryInDb(vehicle.Id);
        var command = new DeleteEnergyEntryCommand(fuelEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(vehicle.Id));
        
        var existingEntry = Context.EnergyEntries.FirstOrDefault(fe => fe.Id == fuelEntry.Id);
        existingEntry.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_EnergyEntryNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(PowerType.Gasoline);
        var nonExistentEntryId = Guid.NewGuid();
        var command = new DeleteEnergyEntryCommand(nonExistentEntryId, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound(nonExistentEntryId));
    }

    [Fact]
    public async Task Handle_EnergyEntryBelongsToDifferentVehicle_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle1 = await CreateVehicleInDb(PowerType.Gasoline);
        var vehicle2 = await CreateVehicleInDb(PowerType.Electric);
        var fuelEntry = await CreateFuelEntryInDb(vehicle1.Id);
        
        // Try to delete fuel entry using vehicle2's ID
        var command = new DeleteEnergyEntryCommand(fuelEntry.Id, vehicle2.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound(fuelEntry.Id));
        
        var existingEntry = Context.EnergyEntries.FirstOrDefault(fe => fe.Id == fuelEntry.Id);
        existingEntry.ShouldNotBeNull();
    }

    private async Task<Vehicle> CreateVehicleInDb(PowerType powerType, Guid? userId = null)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "TestBrand",
            Model = "TestModel",
            PowerType = powerType,
            UserId = userId ?? LoggedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private async Task<FuelEntry> CreateFuelEntryInDb(Guid vehicleId)
    {
        var fuelEntry = new FuelEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Mileage = 10000,
            Cost = 50.00m,
            Volume = 40.50m,
            Unit = VolumeUnit.Liters,
            PricePerUnit = 1.25m
        };

        Context.EnergyEntries.Add(fuelEntry);
        await Context.SaveChangesAsync();
        return fuelEntry;
    }

    private async Task<ChargingEntry> CreateChargingEntryInDb(Guid vehicleId)
    {
        var chargingEntry = new ChargingEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Mileage = 50000,
            Cost = 85.30m,
            EnergyAmount = 42.5m,
            Unit = EnergyUnit.kWh,
            PricePerUnit = 2.01m,
            ChargingDurationMinutes = 90
        };

        Context.EnergyEntries.Add(chargingEntry);
        await Context.SaveChangesAsync();
        return chargingEntry;
    }
}