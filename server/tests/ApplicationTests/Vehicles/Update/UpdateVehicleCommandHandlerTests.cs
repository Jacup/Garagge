using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.Update;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests.Vehicles.Update;

public class UpdateVehicleCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateVehicleCommandHandler _sut;
    private readonly Mock<IVehicleUpdateValidationService> _vehicleUpdateValidationServiceMock = new();
    private readonly Mock<ILogger<UpdateVehicleCommandHandler>> _loggerMock = new();

    private readonly Result<VehicleEnergyTypesUpdatePlan> _noChangesResult = VehicleEnergyTypesUpdatePlan.NoChanges();

    private readonly Result<VehicleEnergyTypesUpdatePlan> _defaultUpdateValidationFailure = Result.Failure<VehicleEnergyTypesUpdatePlan>(
        VehicleErrors.CannotRemoveEnergyTypes(new List<EnergyType>(), 0));

    public UpdateVehicleCommandHandlerTests()
    {
        _sut = new UpdateVehicleCommandHandler(Context, UserContextMock.Object, _vehicleUpdateValidationServiceMock.Object);
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
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Bus,
            VIN = "1HGBH41JXMN109111"
        };

        var existingEnergyType = new VehicleEnergyType
        {
            Id = Guid.NewGuid(), VehicleId = vehicleId, EnergyType = EnergyType.Gasoline, Vehicle = existingVehicle
        };

        Context.VehicleEnergyTypes.Add(existingEnergyType);
        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new UpdateVehicleCommand(
            vehicleId,
            "Audi",
            "A4",
            EngineType.Fuel,
            [EnergyType.Diesel],
            2010,
            VehicleType.Car,
            "1HGBH41JXMN109186");


        var changePlan = new VehicleEnergyTypesUpdatePlan([EnergyType.Diesel], [EnergyType.Gasoline], []);
        _vehicleUpdateValidationServiceMock
            .Setup(v => v.ValidateEnergyTypesChangeAsync(existingVehicle, It.IsAny<IEnumerable<EnergyType>>(), CancellationToken.None))
            .ReturnsAsync(changePlan);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("Audi");
        result.Value.Model.ShouldBe("A4");
        result.Value.EngineType.ShouldBe(EngineType.Fuel);
        result.Value.AllowedEnergyTypes.Count().ShouldBe(1);
        result.Value.AllowedEnergyTypes.ShouldContain(EnergyType.Diesel);
        result.Value.ManufacturedYear.ShouldBe(2010);
        result.Value.Type.ShouldBe(VehicleType.Car);
        result.Value.VIN.ShouldBe("1HGBH41JXMN109186");
        result.Value.UserId.ShouldBe(AuthorizedUserId);
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
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        _vehicleUpdateValidationServiceMock
            .Setup(v => v.ValidateEnergyTypesChangeAsync(It.IsAny<Vehicle>(), It.IsAny<IEnumerable<EnergyType>>(), CancellationToken.None))
            .ReturnsAsync(_noChangesResult);

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new UpdateVehicleCommand(
            vehicleId,
            "BMW",
            "X3",
            EngineType.Electric,
            []);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("BMW");
        result.Value.Model.ShouldBe("X3");
        result.Value.EngineType.ShouldBe(EngineType.Electric);
        result.Value.ManufacturedYear.ShouldBeNull();
        result.Value.Type.ShouldBeNull();
        result.Value.VIN.ShouldBeNull();
        result.Value.UserId.ShouldBe(AuthorizedUserId);
    }

    [Fact]
    public async Task Handle_EnergyTypeValidationFails_ReturnsSuccessWithUpdatedVehicleDto()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicleId = Guid.NewGuid();
        var existingVehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = "OldBrand",
            Model = "OldModel",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        _vehicleUpdateValidationServiceMock
            .Setup(v => v.ValidateEnergyTypesChangeAsync(It.IsAny<Vehicle>(), It.IsAny<IEnumerable<EnergyType>>(), CancellationToken.None))
            .ReturnsAsync(_defaultUpdateValidationFailure);

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new UpdateVehicleCommand(
            vehicleId,
            "BMW",
            "X3",
            EngineType.Electric,
            []);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenVehicleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicleId = Guid.NewGuid();
        var command = new UpdateVehicleCommand(vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010);

        _vehicleUpdateValidationServiceMock
            .Setup(v => v.ValidateEnergyTypesChangeAsync(It.IsAny<Vehicle>(), It.IsAny<IEnumerable<EnergyType>>(), CancellationToken.None))
            .ReturnsAsync(_noChangesResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound);
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
            EngineType = EngineType.Fuel,
            UserId = otherUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        _vehicleUpdateValidationServiceMock
            .Setup(v => v.ValidateEnergyTypesChangeAsync(It.IsAny<Vehicle>(), It.IsAny<IEnumerable<EnergyType>>(), CancellationToken.None))
            .ReturnsAsync(_noChangesResult);

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        var command = new UpdateVehicleCommand(vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound);
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
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId,
            ManufacturedYear = 2005,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109111"
        };

        Context.Vehicles.Add(existingVehicle);
        await Context.SaveChangesAsync();

        _vehicleUpdateValidationServiceMock
            .Setup(v => v.ValidateEnergyTypesChangeAsync(It.IsAny<Vehicle>(), It.IsAny<IEnumerable<EnergyType>>(), CancellationToken.None))
            .ReturnsAsync(_noChangesResult);

        var command = new UpdateVehicleCommand(vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010, VehicleType.Car, "1HGBH41JXMN109186");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedVehicle = Context.Vehicles.First(v => v.Id == vehicleId);
        updatedVehicle.Brand.ShouldBe("Audi");
        updatedVehicle.Model.ShouldBe("A4");
        updatedVehicle.EngineType.ShouldBe(EngineType.Fuel);
        updatedVehicle.ManufacturedYear.ShouldBe(2010);
        updatedVehicle.Type.ShouldBe(VehicleType.Car);
        updatedVehicle.VIN.ShouldBe("1HGBH41JXMN109186");
        updatedVehicle.UserId.ShouldBe(AuthorizedUserId);
    }
}