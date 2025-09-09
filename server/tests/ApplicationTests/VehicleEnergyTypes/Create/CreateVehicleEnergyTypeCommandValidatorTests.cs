using Application.VehicleEnergyTypes.Create;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace ApplicationTests.VehicleEnergyTypes.Create;

public class CreateVehicleEnergyTypeCommandValidatorTests
{
    private readonly CreateVehicleEnergyTypeCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var command = new CreateVehicleEnergyTypeCommand(Guid.Empty, EnergyType.Gasoline);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), EnergyType.Gasoline);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VehicleId);
    }

    [Theory]
    [InlineData(EnergyType.Gasoline)]
    [InlineData(EnergyType.Diesel)]
    [InlineData(EnergyType.LPG)]
    [InlineData(EnergyType.CNG)]
    [InlineData(EnergyType.Ethanol)]
    [InlineData(EnergyType.Biofuel)]
    [InlineData(EnergyType.Hydrogen)]
    [InlineData(EnergyType.Electric)]
    public void Validate_WhenEnergyTypeIsValid_ShouldNotHaveError(EnergyType energyType)
    {
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), energyType);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.EnergyType);
    }

    [Fact]
    public void Validate_WhenEnergyTypeIsInvalid_ShouldHaveError()
    {
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), (EnergyType)999);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.EnergyType);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new CreateVehicleEnergyTypeCommand(Guid.NewGuid(), EnergyType.Electric);
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}