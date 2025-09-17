using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Application.Services;
using Application.Vehicles.Create;
using Domain.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace ApplicationTests.Vehicles.Create;

public class CreateVehicleCommandValidatorTests
{
    private readonly CreateVehicleCommandValidator _validator;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly IVehicleEngineCompatibilityService _compatibilityService;

    public CreateVehicleCommandValidatorTests()
    {
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        
        var mockDbContext = new Mock<IApplicationDbContext>();
        _compatibilityService = new VehicleEngineCompatibilityService(mockDbContext.Object);
        
        _dateTimeProvider
            .Setup(o => o.UtcNow)
            .Returns(new DateTime(2024, 01, 25));
        
        _validator = new CreateVehicleCommandValidator(_dateTimeProvider.Object, _compatibilityService);
    }

    [Fact]
    public void Validate_WhenBrandIsEmpty_ShouldHaveError()
    {
        var command = new CreateVehicleCommand(string.Empty, "A4", EngineType.Fuel, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsEmpty_ShouldHaveError()
    {
        var command = new CreateVehicleCommand("Audi", string.Empty, EngineType.Fuel, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Fact]
    public void Validate_WhenBrandIsTooLong_ShouldHaveError()
    {
        var longBrand = new string('A', 65);
        var command = new CreateVehicleCommand(longBrand, "A4", EngineType.Fuel, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsTooLong_ShouldHaveError()
    {
        var longModel = new string('B', 65);
        var command = new CreateVehicleCommand("Audi", longModel, EngineType.Fuel, 2010);
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
        var command = new CreateVehicleCommand("Audi", "A4", engineType, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EngineType);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsInFuture_ShouldHaveError()
    {
        var futureYear = _dateTimeProvider.Object.UtcNow.AddYears(1).Year;
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, futureYear);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsTooOld_ShouldHaveError()
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 1885);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Theory]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Truck)]
    public void Validate_WhenVehicleTypeIsValid_ShouldNotHaveError(VehicleType vehicleType)
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010, vehicleType);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Type);
    }

    [Fact]
    public void Validate_WhenVINHasCorrectLength_ShouldNotHaveError()
    {
        var validVIN = "1HGBH41JXMN109186"; // 17 characters
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010, null, validVIN);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VIN);
    }

    [Theory]
    [InlineData("1HGBH41JXMN10918")] // 16 characters
    [InlineData("1HGBH41JXMN1091866")] // 18 characters
    [InlineData("")] // empty
    public void Validate_WhenVINHasIncorrectLength_ShouldHaveError(string invalidVIN)
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010, null, invalidVIN);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VIN);
    }

    [Fact]
    public void Validate_WhenVINIsNull_ShouldNotHaveError()
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VIN);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010, VehicleType.Car, "1HGBH41JXMN109186");
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenCommandIsValidWithMinimalData_ShouldNotHaveError()
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel);
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }

    #region EnergyTypes Validation Tests

    [Fact]
    public void Validate_WhenEnergyTypesIsNull_ShouldNotHaveError()
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: null);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenEnergyTypesIsEmpty_ShouldNotHaveError()
    {
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: new List<EnergyType>());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Theory]
    [InlineData(EngineType.Fuel, EnergyType.Gasoline)]
    [InlineData(EngineType.Fuel, EnergyType.Diesel)]
    [InlineData(EngineType.Fuel, EnergyType.LPG)]
    [InlineData(EngineType.Electric, EnergyType.Electric)]
    [InlineData(EngineType.Hydrogen, EnergyType.Hydrogen)]
    public void Validate_WhenEnergyTypeIsCompatibleWithEngine_ShouldNotHaveError(EngineType engineType, EnergyType energyType)
    {
        var command = new CreateVehicleCommand("Audi", "A4", engineType, EnergyTypes: [energyType]);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Theory]
    [InlineData(EngineType.Electric, EnergyType.Gasoline)]
    [InlineData(EngineType.Electric, EnergyType.Diesel)]
    [InlineData(EngineType.Fuel, EnergyType.Electric)]
    [InlineData(EngineType.Fuel, EnergyType.Hydrogen)]
    [InlineData(EngineType.Hydrogen, EnergyType.Electric)]
    [InlineData(EngineType.Hydrogen, EnergyType.Gasoline)]
    public void Validate_WhenEnergyTypeIsNotCompatibleWithEngine_ShouldHaveError(EngineType engineType, EnergyType energyType)
    {
        var command = new CreateVehicleCommand("Audi", "A4", engineType, EnergyTypes: [energyType]);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenMultipleCompatibleEnergyTypes_ShouldNotHaveError()
    {
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.Diesel, EnergyType.LPG };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenPlugInHybridWithElectricAndGasoline_ShouldNotHaveError()
    {
        var energyTypes = new[] { EnergyType.Electric, EnergyType.Gasoline };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.PlugInHybrid, EnergyTypes: energyTypes);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenEnergyTypesContainsDuplicates_ShouldHaveError()
    {
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.Gasoline, EnergyType.Diesel };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenEnergyTypesExceedsMaximumCount_ShouldHaveError()
    {
        var energyTypes = new[] 
        { 
            EnergyType.Gasoline, EnergyType.Diesel, EnergyType.LPG, EnergyType.CNG,
            EnergyType.Ethanol, EnergyType.Biofuel, EnergyType.Electric, EnergyType.Hydrogen,
            EnergyType.Gasoline 
        };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenInvalidEnumValue_ShouldHaveError()
    {
        var energyTypes = new[] { (EnergyType)999 };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, EnergyTypes: energyTypes);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EnergyTypes);
    }

    [Fact]
    public void Validate_WhenCommandWithValidEnergyTypes_ShouldNotHaveError()
    {
        var energyTypes = new[] { EnergyType.Gasoline, EnergyType.LPG };
        var command = new CreateVehicleCommand("Audi", "A4", EngineType.Fuel, 2010, VehicleType.Car, "1HGBH41JXMN109186", energyTypes);
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }

    #endregion
}
