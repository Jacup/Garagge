using Application.Vehicles.GetById;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles.GetById;

public class GetVehicleByIdQueryValidatorTests
{
    private readonly GetVehicleByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var query = new GetVehicleByIdQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.VehicleId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var query = new GetVehicleByIdQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }
}

