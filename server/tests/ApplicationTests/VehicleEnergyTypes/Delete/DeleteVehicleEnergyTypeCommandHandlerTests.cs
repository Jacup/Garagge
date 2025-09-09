using Application.Abstractions.Authentication;
using Application.VehicleEnergyTypes;
using Application.VehicleEnergyTypes.Delete;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.VehicleEnergyTypes.Delete;

public class DeleteVehicleEnergyTypeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IUserContext> _userContextMock;
    private readonly DeleteVehicleEnergyTypeCommandHandler _handler;

    public DeleteVehicleEnergyTypeCommandHandlerTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _handler = new DeleteVehicleEnergyTypeCommandHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesVehicleEnergyTypeAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);

        var vehicle = await CreateVehicleInDb(userId);
        var vehicleEnergyType = await CreateVehicleEnergyTypeInDb(vehicle.Id, EnergyType.Gasoline);
        var command = new DeleteVehicleEnergyTypeCommand(vehicleEnergyType.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Context.VehicleEnergyTypes.Any(vet => vet.Id == vehicleEnergyType.Id).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsUnauthorizedError()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserId).Returns(Guid.Empty);
        var command = new DeleteVehicleEnergyTypeCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_VehicleEnergyTypeNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleEnergyTypeId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);

        var command = new DeleteVehicleEnergyTypeCommand(vehicleEnergyTypeId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.NotFound(vehicleEnergyTypeId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);

        var vehicle = await CreateVehicleInDb(otherUserId);
        var vehicleEnergyType = await CreateVehicleEnergyTypeInDb(vehicle.Id, EnergyType.Gasoline);
        var command = new DeleteVehicleEnergyTypeCommand(vehicleEnergyType.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.NotFound(vehicleEnergyType.Id));
    }

    [Fact]
    public async Task Handle_EnergyEntriesExist_ReturnsDeleteFailedEntriesExistsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);

        var vehicle = await CreateVehicleInDb(userId);
        var vehicleEnergyType = await CreateVehicleEnergyTypeInDb(vehicle.Id, EnergyType.Gasoline);
        await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline);

        var command = new DeleteVehicleEnergyTypeCommand(vehicleEnergyType.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.DeleteFailedEntriesExists(vehicleEnergyType.Id));
    }

    private async Task<Vehicle> CreateVehicleInDb(Guid userId, EngineType engineType = EngineType.Fuel)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = engineType,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = userId,
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        return vehicle;
    }

    private async Task<VehicleEnergyType> CreateVehicleEnergyTypeInDb(Guid vehicleId, EnergyType energyType)
    {
        var vehicleEnergyType = new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicleId, EnergyType = energyType };
        
        Context.VehicleEnergyTypes.Add(vehicleEnergyType);
        await Context.SaveChangesAsync();
        
        return vehicleEnergyType;
    }

    private async Task CreateEnergyEntryInDb(Guid vehicleId, EnergyType energyType)
    {
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Type = energyType,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Mileage = 1000,
            Cost = 100,
            PricePerUnit = 2
        };
        
        Context.EnergyEntries.Add(energyEntry);
        await Context.SaveChangesAsync();
    }
}