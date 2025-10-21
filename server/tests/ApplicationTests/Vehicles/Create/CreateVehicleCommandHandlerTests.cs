using Application.Core;
using Application.Vehicles;
using Application.Vehicles.Create;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests.Vehicles.Create;

public class CreateVehicleCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<ILogger<CreateVehicleCommandHandler>> _loggerMock = new();
    private readonly CreateVehicleCommandHandler _handler;

    public CreateVehicleCommandHandlerTests()
    {
        _handler = new CreateVehicleCommandHandler(
            Context,
            UserContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithVehicleDto()
    {
        // Arrange
        SetupAuthorizedUser();

        var command = new CreateVehicleCommand(
            "Audi",
            "A4",
            EngineType.Fuel,
            new List<EnergyType>() {EnergyType.Gasoline, EnergyType.LPG},
            2010,
            VehicleType.Car,
            "1HGBH41JXMN109186");
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("Audi");
        result.Value.Model.ShouldBe("A4");
        result.Value.EngineType.ShouldBe(EngineType.Fuel);
        result.Value.ManufacturedYear.ShouldBe(2010);
        result.Value.Type.ShouldBe(VehicleType.Car);
        result.Value.VIN.ShouldBe("1HGBH41JXMN109186");
        result.Value.AllowedEnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.AllowedEnergyTypes.ShouldContain(EnergyType.LPG);
        result.Value.UserId.ShouldBe(AuthorizedUserId);

        var addedEntity = Context.Vehicles.SingleOrDefault(v => v.Id == result.Value.Id);

        addedEntity.ShouldNotBeNull();
        addedEntity.Brand.ShouldBe("Audi");
        addedEntity.Model.ShouldBe("A4");
        addedEntity.EngineType.ShouldBe(EngineType.Fuel);
        addedEntity.ManufacturedYear.ShouldBe(2010);
        addedEntity.Type.ShouldBe(VehicleType.Car);
        addedEntity.VIN.ShouldBe("1HGBH41JXMN109186");
        addedEntity.AllowedEnergyTypes.ShouldContain(EnergyType.Gasoline);
        addedEntity.AllowedEnergyTypes.ShouldContain(EnergyType.LPG);
        addedEntity.UserId.ShouldBe(AuthorizedUserId);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessWithVehicleDto()
    {
        // Arrange
        SetupAuthorizedUser();

        var command = new CreateVehicleCommand(
            "BMW",
            "X3",
            EngineType.Fuel,
            new List<EnergyType>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("BMW");
        result.Value.Model.ShouldBe("X3");
        result.Value.EngineType.ShouldBe(EngineType.Fuel);
        result.Value.ManufacturedYear.ShouldBeNull();
        result.Value.Type.ShouldBeNull();
        result.Value.VIN.ShouldBeNull();
        result.Value.AllowedEnergyTypes.ShouldBeEmpty();
        result.Value.UserId.ShouldBe(AuthorizedUserId);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, [], 2010);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.Unauthorized);
    }

    [Theory]
    [InlineData(EngineType.Fuel)]
    [InlineData(EngineType.Hybrid)]
    [InlineData(EngineType.PlugInHybrid)]
    [InlineData(EngineType.Electric)]
    [InlineData(EngineType.Hydrogen)]
    public async Task Handle_WithDifferentEngineTypes_CreatesVehicleWithCorrectEngineType(EngineType engineType)
    {
        // Arrange
        SetupAuthorizedUser();
        var command = new CreateVehicleCommand("Audi", "A4", engineType, [EnergyType.CNG], 2010);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.EngineType.ShouldBe(engineType);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesNewGuidForVehicleId()
    {
        // Arrange
        SetupAuthorizedUser();
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, [], 2010);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task Handle_WithEmptyEnergyTypes_DoesNotCreateVehicleEnergyTypes()
    {
        // Arrange
        SetupAuthorizedUser();
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, new List<EnergyType>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.AllowedEnergyTypes.ShouldBeEmpty();
        Context.Vehicles.First().AllowedEnergyTypes.ShouldBeEmpty();
    }
}