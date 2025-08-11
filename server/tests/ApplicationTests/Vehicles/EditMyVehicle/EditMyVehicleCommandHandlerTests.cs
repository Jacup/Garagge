using Application.Abstractions.Data;
using Application.Vehicles;
using Application.Vehicles.EditMyVehicle;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.Vehicles.EditMyVehicle;

public class EditMyVehicleCommandHandlerTests : InMemoryDbTestBase
{
    private readonly EditMyVehicleCommandHandler _sut;

    public EditMyVehicleCommandHandlerTests()
    {
        _sut = new EditMyVehicleCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithUpdatedVehicleDto()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicleId = Guid.NewGuid();
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "OldBrand",
            Model = "OldModel",
            PowerType = PowerType.Gasoline,
            UserId = LoggedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Bus,
            VIN = "1HGBH41JXMN109111"
        };

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new EditMyVehicleCommand(
            vehicleId,
            "Audi",
            "A4",
            PowerType.Diesel,
            2010,
            VehicleType.Car,
            "1HGBH41JXMN109186");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("Audi");
        result.Value.Model.ShouldBe("A4");
        result.Value.PowerType.ShouldBe(PowerType.Diesel);
        result.Value.ManufacturedYear.ShouldBe(2010);
        result.Value.Type.ShouldBe(VehicleType.Car);
        result.Value.VIN.ShouldBe("1HGBH41JXMN109186");
        result.Value.UserId.ShouldBe(LoggedUserId);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessWithUpdatedVehicleDto()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicleId = Guid.NewGuid();
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "OldBrand",
            Model = "OldModel",
            PowerType = PowerType.Gasoline,
            UserId = LoggedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new EditMyVehicleCommand(
            vehicleId,
            "BMW",
            "X3",
            PowerType.Electric);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("BMW");
        result.Value.Model.ShouldBe("X3");
        result.Value.PowerType.ShouldBe(PowerType.Electric);
        result.Value.ManufacturedYear.ShouldBeNull();
        result.Value.Type.ShouldBeNull();
        result.Value.VIN.ShouldBeNull();
        result.Value.UserId.ShouldBe(LoggedUserId);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ReturnsUnauthorizedError()
    {
        // Arrange
        UserContextMock
            .Setup(o => o.UserId)
            .Returns(Guid.Empty);
            
        var command = new EditMyVehicleCommand(Guid.NewGuid(), "Audi", "A4", PowerType.Gasoline, 2010);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicleId = Guid.NewGuid();
        var command = new EditMyVehicleCommand(vehicleId, "Audi", "A4", PowerType.Gasoline, 2010);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(vehicleId));
    }

    [Fact]
    public async Task Handle_WhenVehicleBelongsToAnotherUser_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var otherUserId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "OldBrand",
            Model = "OldModel",
            PowerType = PowerType.Gasoline,
            UserId = otherUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new EditMyVehicleCommand(vehicleId, "Audi", "A4", PowerType.Gasoline, 2010);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(vehicleId));
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ReturnsDeleteFailedError()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicleId = Guid.NewGuid();
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "OldBrand",
            Model = "OldModel",
            PowerType = PowerType.Gasoline,
            UserId = LoggedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var applicationDbContextMock = new Mock<IApplicationDbContext>();
        
        applicationDbContextMock
            .Setup(o => o.Vehicles)
            .Returns(Context.Vehicles);
        applicationDbContextMock
            .Setup(o => o.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Throws(new Exception("Database error"));

        var mockedSut = new EditMyVehicleCommandHandler(applicationDbContextMock.Object, UserContextMock.Object);
        var command = new EditMyVehicleCommand(vehicleId, "Audi", "A4", PowerType.Gasoline, 2010);

        // Act
        var result = await mockedSut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.EditFailed(vehicleId));
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesVehicleInDatabase()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var vehicleId = Guid.NewGuid();
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "OldBrand",
            Model = "OldModel",
            PowerType = PowerType.Gasoline,
            UserId = LoggedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new EditMyVehicleCommand(vehicleId, "Audi", "A4", PowerType.Diesel, 2010, VehicleType.Car, "1HGBH41JXMN109186");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedVehicle = Context.Vehicles.First(v => v.Id == vehicleId);
        updatedVehicle.Brand.ShouldBe("Audi");
        updatedVehicle.Model.ShouldBe("A4");
        updatedVehicle.PowerType.ShouldBe(PowerType.Diesel);
        updatedVehicle.ManufacturedYear.ShouldBe(2010);
        updatedVehicle.Type.ShouldBe(VehicleType.Car);
        updatedVehicle.VIN.ShouldBe("1HGBH41JXMN109186");
        updatedVehicle.UserId.ShouldBe(LoggedUserId);
    }
}