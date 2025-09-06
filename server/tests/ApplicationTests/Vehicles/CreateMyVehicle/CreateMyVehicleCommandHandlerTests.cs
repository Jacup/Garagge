using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Vehicles;
using Application.Vehicles.CreateMyVehicle;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApplicationTests.Vehicles.CreateMyVehicle;

public class CreateMyVehicleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<DbSet<Vehicle>> _vehicleDbSetMock;
    private readonly CreateMyVehicleCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateMyVehicleCommandHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _userContextMock = new Mock<IUserContext>();
        _vehicleDbSetMock = new Mock<DbSet<Vehicle>>();
        
        _dbContextMock.Setup(x => x.Vehicles).Returns(_vehicleDbSetMock.Object);
        _userContextMock.Setup(x => x.UserId).Returns(_userId);
        
        _handler = new CreateMyVehicleCommandHandler(_dbContextMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithVehicleDto()
    {
        // Arrange
        var command = new CreateMyVehicleCommand(
            "Audi", 
            "A4", 
            EngineType.Fuel, 
            2010, 
            VehicleType.Car, 
            "1HGBH41JXMN109186");

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

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
        result.Value.UserId.ShouldBe(_userId);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessWithVehicleDto()
    {
        // Arrange
        var command = new CreateMyVehicleCommand(
            "BMW", 
            "X3", 
            EngineType.Fuel);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

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
        result.Value.UserId.ShouldBe(_userId);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ReturnsUnauthorizedError()
    {
        // Arrange
        var command = new CreateMyVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);
        
        _userContextMock
            .Setup(x => x.UserId)
            .Returns(Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsCreateFailedError()
    {
        // Arrange
        var command = new CreateMyVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.CreateFailed);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddAsyncOnVehiclesDbSet()
    {
        // Arrange
        var command = new CreateMyVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _vehicleDbSetMock.Verify(x => x.AddAsync(
            It.Is<Vehicle>(v => 
                v.Brand == "Audi" && 
                v.Model == "A4" && 
                v.EngineType == EngineType.Fuel &&
                v.ManufacturedYear == 2010 &&
                v.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsSaveChangesAsync()
    {
        // Arrange
        var command = new CreateMyVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        var command = new CreateMyVehicleCommand("Audi", "A4", engineType, 2010);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

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
        var command = new CreateMyVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }
}
