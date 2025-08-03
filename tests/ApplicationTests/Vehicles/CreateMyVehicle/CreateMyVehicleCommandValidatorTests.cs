using Application.Abstractions;
using Application.Vehicles.CreateMyVehicle;
using FluentValidation.TestHelper;
using Moq;

namespace ApplicationTests.Vehicles.CreateMyVehicle;

public class CreateMyVehicleCommandValidatorTests
{
    private readonly CreateMyVehicleCommandValidator _validator;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;

    public CreateMyVehicleCommandValidatorTests()
    {
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        
        _dateTimeProvider
            .Setup(o => o.UtcNow)
            .Returns(new DateTime(2024, 01, 25));
        
        _validator =  new CreateMyVehicleCommandValidator(_dateTimeProvider.Object);
    }


    [Fact]
    public void Validate_WhenBrandIsEmpty_ShouldHaveError()
    {
        var command = new CreateMyVehicleCommand(String.Empty, "A4", 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsEmpty_ShouldHaveError()
    {
        var command = new CreateMyVehicleCommand("Audi", String.Empty, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Fact]
    public void Validate_WhenBrandIsTooLong_ShouldHaveError()
    {
        var longBrand = new string('A', 65);
        var command = new CreateMyVehicleCommand(longBrand, "A4", 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsTooLong_ShouldHaveError()
    {
        var longModel = new string('B', 65);
        var command = new CreateMyVehicleCommand("Audi", longModel, 2010);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsInFuture_ShouldHaveError()
    {
        var futureYear = _dateTimeProvider.Object.UtcNow.AddYears(1).Year;
        var command = new CreateMyVehicleCommand("Audi", "A4", futureYear);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new CreateMyVehicleCommand("Audi", "A4", 2010);
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}
