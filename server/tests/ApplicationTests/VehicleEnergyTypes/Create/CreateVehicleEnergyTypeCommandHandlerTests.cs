using Application.Abstractions.Authentication;
using Application.Abstractions.Services;
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
    private readonly Mock<IVehicleEngineCompatibilityService> _compatibilityServiceMock;
    private readonly CreateVehicleEnergyTypeCommandHandler _handler;

    public CreateVehicleEnergyTypeCommandHandlerTests()
    {
        _userContextMock = new Mock<IUserContext>();
        _compatibilityServiceMock = new Mock<IVehicleEngineCompatibilityService>();
        _handler = new CreateVehicleEnergyTypeCommandHandler(Context, _userContextMock.Object, _compatibilityServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsEnergyTypeAndReturnsDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock
            .Setup(x => x.UserId)
            .Returns(userId);
        
        _compatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleWithEngine(EnergyType.Gasoline, EngineType.Fuel))
            .Returns(true);
        
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
        _userContextMock
            .Setup(x => x.UserId)
            .Returns(userId);
        
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
        _userContextMock
            .Setup(x => x.UserId)
            .Returns(userId);
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
    public async Task Handle_EnergyTypeIncompatibleWithEngine_ReturnsIncompatibleWithEngineError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock
            .Setup(x => x.UserId)
            .Returns(userId);
        
        _compatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleWithEngine(EnergyType.Electric, EngineType.Fuel))
            .Returns(false);
        
        var vehicle = await CreateVehicleInDb(userId);
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Electric);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.IncompatibleWithEngine(EnergyType.Electric, EngineType.Fuel));
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
    
    [Theory]
    [InlineData(EngineType.Electric, EnergyType.Electric, true)]
    [InlineData(EngineType.Fuel, EnergyType.Gasoline, true)]
    [InlineData(EngineType.Fuel, EnergyType.Diesel, true)]
    [InlineData(EngineType.PlugInHybrid, EnergyType.Electric, true)]
    [InlineData(EngineType.PlugInHybrid, EnergyType.Gasoline, true)]
    public async Task Handle_CompatibleEnergyTypeWithEngine_AddsEnergyTypeAndReturnsDto(EngineType engineType, EnergyType energyType, bool isCompatible)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _compatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleWithEngine(energyType, engineType))
            .Returns(isCompatible);
        
        var vehicle = await CreateVehicleInDb(userId, engineType);
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, energyType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.EnergyType.ShouldBe(energyType);
        Context.VehicleEnergyTypes.Any(vet => vet.VehicleId == vehicle.Id && vet.EnergyType == energyType).ShouldBeTrue();
    }

    [Theory]
    [InlineData(EngineType.Electric, EnergyType.Gasoline)]
    [InlineData(EngineType.Fuel, EnergyType.Electric)]
    [InlineData(EngineType.Hydrogen, EnergyType.Gasoline)]
    [InlineData(EngineType.Electric, EnergyType.Hydrogen)]
    public async Task Handle_IncompatibleEnergyTypeWithEngine_ReturnsIncompatibleWithEngineError(EngineType engineType, EnergyType energyType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _compatibilityServiceMock
            .Setup(x => x.IsEnergyTypeCompatibleWithEngine(energyType, engineType))
            .Returns(false);
        
        var vehicle = await CreateVehicleInDb(userId, engineType);
        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, energyType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.IncompatibleWithEngine(energyType, engineType));
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
}
