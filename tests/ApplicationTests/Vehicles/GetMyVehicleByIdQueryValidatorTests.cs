using Application.Vehicles.GetMyVehicleById;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles;

public class GetMyVehicleByIdQueryValidatorTests
{
    private readonly GetMyVehicleByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        var query = new GetMyVehicleByIdQuery(Guid.NewGuid(), Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.VehicleId);
    }
    
    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveError()
    {
        var query = new GetMyVehicleByIdQuery(Guid.Empty, Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.UserId);
    }

    [Fact]
    public void Validate_WhenVehicleIdIsValid_ShouldNotHaveError()
    {
        var query = new GetMyVehicleByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }
}

