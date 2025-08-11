using Application.Vehicles.DeleteMyVehicleById;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles.DeleteMyVehicleById;

public class DeleteMyVehicleByIdCommandValidatorTests
{
    private readonly DeleteMyVehicleByIdCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var command = new DeleteMyVehicleByIdCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var command = new DeleteMyVehicleByIdCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}

