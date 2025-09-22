using Application.VehicleEnergyTypes.Create;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace ApplicationTests.VehicleEnergyTypes.Create;

public class CreateVehicleEnergyTypeCommandValidatorTests : InMemoryDbTestBase
{
    private readonly CreateVehicleEnergyTypeCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var command = new CreateVehicleEnergyTypeCommand(Guid.Empty, EnergyType.Gasoline);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
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
    public void Validate_WhenEnergyTypeIsValid_ShouldNotHaveEnumError(EnergyType energyType)
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
}