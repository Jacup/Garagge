using Application.VehicleEnergyTypes.Delete;
using FluentValidation.TestHelper;

namespace ApplicationTests.VehicleEnergyTypes.Delete;

public class DeleteVehicleEnergyTypeCommandValidatorTests
{
    private readonly DeleteVehicleEnergyTypeCommandValidator _validator;

    public DeleteVehicleEnergyTypeCommandValidatorTests()
    {
        _validator = new DeleteVehicleEnergyTypeCommandValidator();
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_ShouldHaveError()
    {
        var command = new DeleteVehicleEnergyTypeCommand(Guid.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_WhenIdIsValid_ShouldNotHaveError()
    {
        var command = new DeleteVehicleEnergyTypeCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var command = new DeleteVehicleEnergyTypeCommand(Guid.NewGuid(), Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var command = new DeleteVehicleEnergyTypeCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenBothIdsAreEmpty_ShouldHaveErrors()
    {
        var command = new DeleteVehicleEnergyTypeCommand(Guid.Empty, Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Id);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveError()
    {
        var command = new DeleteVehicleEnergyTypeCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}
