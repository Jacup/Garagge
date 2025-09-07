using Application.Abstractions.Authentication;
using Application.VehicleEnergyTypes;
using Application.VehicleEnergyTypes.Create;
using Application.Vehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.VehicleEnergyTypes.Create;

public class CreateVehicleEnergyTypeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CreateVehicleEnergyTypeCommandHandler _handler;

    public CreateVehicleEnergyTypeCommandHandlerTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _handler = new CreateVehicleEnergyTypeCommandHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsEnergyTypeAndReturnsDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        var vehicle = await CreateVehicleInDb(userId);
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.EnergyType.ShouldBe(EnergyType.Gasoline);
        Context.VehicleEnergyTypes.Any(vet => vet.VehicleId == vehicle.Id && vet.EnergyType == EnergyType.Gasoline).ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(command.VehicleId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsUnauthorizedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        var vehicle = await CreateVehicleInDb(otherUserId);
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_EnergyTypeAlreadyExists_ReturnsAlreadyExistsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        var vehicle = await CreateVehicleInDb(userId);
        var existingVet = new VehicleEnergyType { VehicleId = vehicle.Id, EnergyType = EnergyType.Gasoline };
        Context.VehicleEnergyTypes.Add(existingVet);
        await Context.SaveChangesAsync();
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.AlreadyExists(vehicle.Id, EnergyType.Gasoline));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsUnauthorizedError()
    {
        // Arrange
        _userContextMock.Setup(x => x.UserId).Returns(Guid.Empty);
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_DbException_ReturnsCreateFailedError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        var vehicle = await CreateVehicleInDb(userId);
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);
        // Wymuszenie wyjątku przez zamknięcie kontekstu
        await Context.DisposeAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.CreateFailed);
    }

    private async Task<Vehicle> CreateVehicleInDb(Guid userId)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = userId,
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }
}

