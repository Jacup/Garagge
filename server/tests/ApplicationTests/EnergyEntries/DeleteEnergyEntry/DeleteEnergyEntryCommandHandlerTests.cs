using Application.EnergyEntries;
using Application.EnergyEntries.Delete;
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
    public async Task Handle_ValidEnergyEntryDeletion_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var energyEntry = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline);
        var command = new DeleteEnergyEntryCommand(energyEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var deletedEntry = Context.EnergyEntries.FirstOrDefault(ee => ee.Id == energyEntry.Id);
        deletedEntry.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_EnergyEntryNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var nonExistentEntryId = Guid.NewGuid();
        var command = new DeleteEnergyEntryCommand(nonExistentEntryId, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound(nonExistentEntryId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline, otherUserId);
        var energyEntry = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline);
        var command = new DeleteEnergyEntryCommand(energyEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound(vehicle.Id));
    }

    [Fact]
    public async Task Handle_EnergyEntryBelongsToDifferentVehicle_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle1 = await CreateVehicleInDb(EnergyType.Gasoline);
        var vehicle2 = await CreateVehicleInDb(EnergyType.Gasoline);
        var energyEntry = await CreateEnergyEntryInDb(vehicle1.Id, EnergyType.Gasoline);
        var command = new DeleteEnergyEntryCommand(energyEntry.Id, vehicle2.Id); // Wrong vehicle ID

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound(energyEntry.Id));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailureWithUnauthorizedError()
    {
        // Arrange
        UserContextMock.Setup(x => x.UserId).Returns(Guid.Empty); // Unauthorized user
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var energyEntry = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline);
        var command = new DeleteEnergyEntryCommand(energyEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_ElectricEnergyEntryDeletion_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Electric);
        var energyEntry = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Electric);
        var command = new DeleteEnergyEntryCommand(energyEntry.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var deletedEntry = Context.EnergyEntries.FirstOrDefault(ee => ee.Id == energyEntry.Id);
        deletedEntry.ShouldBeNull();
    }

    private async Task<Vehicle> CreateVehicleInDb(EnergyType energyType, Guid? userId = null)
    {
        var vehicleId = Guid.NewGuid();
        
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = userId ?? LoggedUserId,
            VehicleEnergyTypes = new List<VehicleEnergyType>
            {
                new VehicleEnergyType
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicleId,
                    EnergyType = energyType
                }
            }
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private async Task<EnergyEntry> CreateEnergyEntryInDb(Guid vehicleId, EnergyType energyType)
    {
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Date = new DateOnly(2023, 10, 14),
            Mileage = 1000,
            Type = energyType,
            EnergyUnit = energyType == EnergyType.Electric ? EnergyUnit.kWh : EnergyUnit.Liter,
            Volume = 50m,
            Cost = 100m,
            PricePerUnit = 2m
        };

        Context.EnergyEntries.Add(energyEntry);
        await Context.SaveChangesAsync();
        return energyEntry;
    }
}