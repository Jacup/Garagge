using Application.Vehicles.Delete;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles.Delete;

public class DeleteVehicleByIdCommandValidatorTests
{
    private readonly DeleteVehicleByIdCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var command = new DeleteVehicleByIdCommand(Guid.Empty);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.VehicleId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var command = new DeleteVehicleByIdCommand(Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.IsValid.ShouldBeTrue();
    }
}

