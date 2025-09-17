using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Vehicles;
using Application.Vehicles.Create;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ApplicationTests.Vehicles.Create;

public class CreateVehicleCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<DbSet<Vehicle>> _vehicleDbSetMock;
    private readonly Mock<DbSet<VehicleEnergyType>> _vehicleEnergyTypeDbSetMock;
    private readonly CreateVehicleCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateVehicleCommandHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _userContextMock = new Mock<IUserContext>();
        _vehicleDbSetMock = new Mock<DbSet<Vehicle>>();
        _vehicleEnergyTypeDbSetMock = new Mock<DbSet<VehicleEnergyType>>();
        
        _dbContextMock.Setup(x => x.Vehicles).Returns(_vehicleDbSetMock.Object);
        _dbContextMock.Setup(x => x.VehicleEnergyTypes).Returns(_vehicleEnergyTypeDbSetMock.Object);
        _userContextMock.Setup(x => x.UserId).Returns(_userId);
        
        _handler = new CreateVehicleCommandHandler(_dbContextMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithVehicleDto()
    {
        // Arrange
        var command = new CreateVehicleCommand(
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
        var command = new CreateVehicleCommand(
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
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);
        
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
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

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
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

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
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

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
        var command = new CreateVehicleCommand("Audi", "A4", engineType, 2010);

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
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithEnergyTypes_CreatesVehicleEnergyTypesAndSetsNavigationProperty()
    {
        // Arrange
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.Diesel };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.Is<IEnumerable<VehicleEnergyType>>(vets => 
                vets.Count() == 2 &&
                vets.Any(vet => vet.EnergyType == EnergyType.Gasoline) &&
                vets.Any(vet => vet.EnergyType == EnergyType.Diesel) &&
                vets.All(vet => vet.VehicleId != Guid.Empty && vet.Id != Guid.Empty)),
            It.IsAny<CancellationToken>()), Times.Once);

        result.Value.AllowedEnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.AllowedEnergyTypes.ShouldContain(EnergyType.Diesel);
        result.Value.AllowedEnergyTypes.Count().ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WithNullEnergyTypes_DoesNotCreateVehicleEnergyTypes()
    {
        // Arrange
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: null);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.IsAny<IEnumerable<VehicleEnergyType>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        result.Value.AllowedEnergyTypes.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyEnergyTypes_DoesNotCreateVehicleEnergyTypes()
    {
        // Arrange
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: new List<EnergyType>());

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.IsAny<IEnumerable<VehicleEnergyType>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        result.Value.AllowedEnergyTypes.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateEnergyTypes_CreatesDedupedVehicleEnergyTypes()
    {
        // Arrange
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.Diesel, EnergyType.Gasoline, EnergyType.LPG };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.Is<IEnumerable<VehicleEnergyType>>(vets => 
                vets.Count() == 3 &&
                vets.Count(vet => vet.EnergyType == EnergyType.Gasoline) == 1 && 
                vets.Any(vet => vet.EnergyType == EnergyType.Diesel) &&
                vets.Any(vet => vet.EnergyType == EnergyType.LPG)),
            It.IsAny<CancellationToken>()), Times.Once);

        result.Value.AllowedEnergyTypes.Count().ShouldBe(3);
        result.Value.AllowedEnergyTypes.Count(et => et == EnergyType.Gasoline).ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WithSingleEnergyType_CreatesOneVehicleEnergyType()
    {
        // Arrange
        var energyTypes = new[] { EnergyType.Electric };
        var command = new CreateVehicleCommand("Tesla", "Model 3", EngineType.Electric, EnergyTypes: energyTypes);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.Is<IEnumerable<VehicleEnergyType>>(vets => 
                vets.Count() == 1 &&
                vets.First().EnergyType == EnergyType.Electric &&
                vets.First().VehicleId != Guid.Empty),
            It.IsAny<CancellationToken>()), Times.Once);

        result.Value.AllowedEnergyTypes.ShouldHaveSingleItem();
        result.Value.AllowedEnergyTypes.First().ShouldBe(EnergyType.Electric);
    }

    [Fact]
    public async Task Handle_WithMultipleEnergyTypes_AssignsCorrectVehicleIdToAllEnergyTypes()
    {
        // Arrange
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.Diesel, EnergyType.LPG };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);

        Vehicle createdVehicle = null!;
        _vehicleDbSetMock
            .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) => createdVehicle = v);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        createdVehicle.ShouldNotBeNull();
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.Is<IEnumerable<VehicleEnergyType>>(vets => 
                vets.All(vet => vet.VehicleId == createdVehicle.Id) &&
                vets.All(vet => vet.Id != Guid.Empty)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEnergyTypesProvidedButSaveFails_StillReturnsCreateFailedError()
    {
        // Arrange
        var energyTypes = new[] { EnergyType.Gasoline };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.CreateFailed);
        
        _vehicleEnergyTypeDbSetMock.Verify(x => x.AddRangeAsync(
            It.IsAny<IEnumerable<VehicleEnergyType>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EnsuresNavigationPropertyIsSetForMapsterMapping()
    {
        // Arrange
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.Diesel };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);

        Vehicle vehiclePassedToAdd = null!;
        _vehicleDbSetMock
            .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, ct) => vehiclePassedToAdd = v);

        _dbContextMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        vehiclePassedToAdd.ShouldNotBeNull();
        
        vehiclePassedToAdd.VehicleEnergyTypes.ShouldNotBeNull();
        vehiclePassedToAdd.VehicleEnergyTypes.Count.ShouldBe(2);
        vehiclePassedToAdd.VehicleEnergyTypes.ShouldContain(vet => vet.EnergyType == EnergyType.Gasoline);
        vehiclePassedToAdd.VehicleEnergyTypes.ShouldContain(vet => vet.EnergyType == EnergyType.Diesel);
        
        vehiclePassedToAdd.AllowedEnergyTypes.ShouldContain(EnergyType.Gasoline);
        vehiclePassedToAdd.AllowedEnergyTypes.ShouldContain(EnergyType.Diesel);
        vehiclePassedToAdd.AllowedEnergyTypes.Count().ShouldBe(2);
    }
}
