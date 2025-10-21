using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Services;
using Application.Vehicles.Update;
using Domain.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace ApplicationTests.Vehicles.Update;

public class UpdateVehicleCommandValidatorTests
{
    private readonly UpdateVehicleCommandValidator _validator;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly Guid _vehicleId = Guid.NewGuid();

    public UpdateVehicleCommandValidatorTests()
    {
        var mockDbContext = new Mock<IApplicationDbContext>();
        IVehicleEngineCompatibilityService compatibilityService = new VehicleEngineCompatibilityService(mockDbContext.Object);
        
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        
        _dateTimeProvider
            .Setup(o => o.UtcNow)
            .Returns(new DateTime(2024, 01, 25));
        
        _validator = new UpdateVehicleCommandValidator(_dateTimeProvider.Object, compatibilityService);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var command = new UpdateVehicleCommand(Guid.Empty, "Audi", "A4", EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenBrandIsEmpty_ShouldHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, string.Empty, "A4", EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsEmpty_ShouldHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", string.Empty, EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Fact]
    public void Validate_WhenBrandIsTooLong_ShouldHaveError()
    {
        var longBrand = new string('A', 65);
        var command = new UpdateVehicleCommand(_vehicleId, longBrand, "A4", EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsTooLong_ShouldHaveError()
    {
        var longModel = new string('B', 65);
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", longModel, EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Theory]
    [InlineData(EngineType.Fuel)]
    [InlineData(EngineType.Hybrid)]
    [InlineData(EngineType.PlugInHybrid)]
    [InlineData(EngineType.Electric)]
    public void Validate_WhenEngineTypeIsValid_ShouldNotHaveError(EngineType engineType)
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", engineType, []);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EngineType);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsInFuture_ShouldHaveError()
    {
        var futureYear = _dateTimeProvider.Object.UtcNow.AddYears(1).Year;
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], futureYear);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsTooOld_ShouldHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 1885);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsNull_ShouldNotHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Theory]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Truck)]
    public void Validate_WhenVehicleTypeIsValid_ShouldNotHaveError(VehicleType vehicleType)
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010, vehicleType);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Validate_WhenVehicleTypeIsNull_ShouldNotHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010, null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Validate_WhenVINHasCorrectLength_ShouldNotHaveError()
    {
        var validVIN = "1HGBH41JXMN109186"; // 17 characters
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010, null, validVIN);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VIN);
    }

    [Theory]
    [InlineData("1HGBH41JXMN10918")] // 16 characters
    [InlineData("1HGBH41JXMN1091866")] // 18 characters
    [InlineData("")] // empty
    public void Validate_WhenVINHasIncorrectLength_ShouldHaveError(string invalidVIN)
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010, null, invalidVIN);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VIN);
    }

    [Fact]
    public void Validate_WhenVINIsNull_ShouldNotHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VIN);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, [], 2010, VehicleType.Car, "1HGBH41JXMN109186");
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenCommandIsValidWithMinimalData_ShouldNotHaveError()
    {
        var command = new UpdateVehicleCommand(_vehicleId, "Audi", "A4", EngineType.Fuel, []);
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}
