using Application.Vehicles.GetMyVehicles;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles;

public class GetMyVehiclesQueryValidatorTests
{
    private readonly GetMyVehiclesQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveError()
    {
        var query = new GetMyVehicles(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.UserId);
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveError()
    {
        var query = new GetMyVehicles(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }
}

