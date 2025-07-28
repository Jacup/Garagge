using Application.Vehicles.CreateMyVehicle;
using FluentValidation.TestHelper;
using Shouldly;

namespace ApplicationTests.Vehicles;

public class CreateMyVehicleCommandValidatorTests
{
    private readonly CreateMyVehicleCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenBrandIsEmpty_ShouldHaveError()
    {
        var command = new CreateMyVehicleCommand(String.Empty, "A4", new DateOnly(2010, 01, 20));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsEmpty_ShouldHaveError()
    {
        var command = new CreateMyVehicleCommand("Audi", String.Empty, new DateOnly(2010, 01, 20));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Fact]
    public void Validate_WhenBrandIsTooLong_ShouldHaveError()
    {
        var longBrand = new string('A', 65);
        var command = new CreateMyVehicleCommand(longBrand, "A4", new DateOnly(2010, 01, 20));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Brand);
    }

    [Fact]
    public void Validate_WhenModelIsTooLong_ShouldHaveError()
    {
        var longModel = new string('B', 65);
        var command = new CreateMyVehicleCommand("Audi", longModel, new DateOnly(2010, 01, 20));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Model);
    }

    [Fact]
    public void Validate_WhenManufacturedYearIsInFuture_ShouldHaveError()
    {
        var futureYear = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
        var command = new CreateMyVehicleCommand("Audi", "A4", futureYear);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ManufacturedYear);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new CreateMyVehicleCommand("Audi", "A4", new DateOnly(2010, 01, 20));
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}
