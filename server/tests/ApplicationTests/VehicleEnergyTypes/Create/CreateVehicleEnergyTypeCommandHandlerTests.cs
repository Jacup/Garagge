using Application.Abstractions.Authentication;
using Application.Abstractions.Services;
using Application.Core;
using Application.VehicleEnergyTypes;
using Application.VehicleEnergyTypes.Create;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests.VehicleEnergyTypes.Create;

public class CreateVehicleEnergyTypeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly CreateVehicleEnergyTypeCommandHandler _handler;
    private readonly Mock<IVehicleEngineCompatibilityService> _vehicleEngineCompatibilityServiceMock = new();
    private readonly Mock<ILogger<CreateVehicleEnergyTypeCommandHandler>> _loggerMock = new();

    public CreateVehicleEnergyTypeCommandHandlerTests()
    {
        _handler = new CreateVehicleEnergyTypeCommandHandler(
            Context,
            UserContextMock.Object,
            _vehicleEngineCompatibilityServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommandAndVehicleExists_AddsEnergyTypeAndReturnsDto()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        _vehicleEngineCompatibilityServiceMock
            .Setup(service => service.ValidateEnergyTypeAssignment(It.IsAny<Vehicle>(), It.IsAny<EnergyType>()))
            .Returns(Result.Success());

        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.VehicleId.ShouldBe(command.VehicleId);
        result.Value.EnergyType.ShouldBe(EnergyType.Gasoline);

        var createdEntity = Context.VehicleEnergyTypes.First();
        createdEntity.VehicleId.ShouldBe(command.VehicleId);
        createdEntity.EnergyType.ShouldBe(EnergyType.Gasoline);
        createdEntity.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.Unauthorized);
        Context.VehicleEnergyTypes.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(EnergyType.Gasoline)]
    [InlineData(EnergyType.Diesel)]
    [InlineData(EnergyType.LPG)]
    [InlineData(EnergyType.Electric)]
    [InlineData(EnergyType.Hydrogen)]
    public async Task Handle_ValidEnergyType_CreatesCorrectEnergyType(EnergyType energyType)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        _vehicleEngineCompatibilityServiceMock
            .Setup(service => service.ValidateEnergyTypeAssignment(It.IsAny<Vehicle>(), energyType))
            .Returns(Result.Success());

        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, energyType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.EnergyType.ShouldBe(energyType);

        var createdEntity = Context.VehicleEnergyTypes.First();
        createdEntity.EnergyType.ShouldBe(energyType);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesUniqueId()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        _vehicleEngineCompatibilityServiceMock
            .Setup(service => service.ValidateEnergyTypeAssignment(It.IsAny<Vehicle>(), It.IsAny<EnergyType>()))
            .Returns(Result.Success());

        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);

        var createdEntity = Context.VehicleEnergyTypes.First();
        createdEntity.Id.ShouldNotBe(Guid.Empty);
        createdEntity.Id.ShouldBe(result.Value.Id);
    }

    [Fact]
    public async Task Handle_ValidCommand_UsesMapsterForMapping()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        _vehicleEngineCompatibilityServiceMock
            .Setup(service => service.ValidateEnergyTypeAssignment(It.IsAny<Vehicle>(), It.IsAny<EnergyType>()))
            .Returns(Result.Success());
        

        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Diesel);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.EnergyType.ShouldBe(EnergyType.Diesel);

        var entity = Context.VehicleEnergyTypes.First();
        entity.Id.ShouldBe(result.Value.Id);
        entity.VehicleId.ShouldBe(result.Value.VehicleId);
        entity.EnergyType.ShouldBe(result.Value.EnergyType);
    }
    
    [Fact]
    public async Task Handle_VehicleDoesNotExists_AddsEnergyTypeAndReturnsDto()
    {
        // Arrange
        SetupAuthorizedUser();
        
        _vehicleEngineCompatibilityServiceMock
            .Setup(service => service.ValidateEnergyTypeAssignment(It.IsAny<Vehicle>(), It.IsAny<EnergyType>()))
            .Returns(Result.Success());

        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(VehicleEnergyTypeErrors.VehicleNotFound(command.VehicleId));
    }

    [Fact]
    public async Task Handle_EnergyTypeIsNotCompatible_AddsEnergyTypeAndReturnsDto()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        _vehicleEngineCompatibilityServiceMock
            .Setup(service => service.ValidateEnergyTypeAssignment(It.IsAny<Vehicle>(), It.IsAny<EnergyType>()))
            .Returns(Result.Failure(It.IsAny<Error>()));

        var command = new CreateVehicleEnergyTypeCommand(vehicle.Id, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
    }
}