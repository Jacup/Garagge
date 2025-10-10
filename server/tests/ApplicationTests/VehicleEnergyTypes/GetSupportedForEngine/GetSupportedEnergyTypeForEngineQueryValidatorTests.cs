using Application.VehicleEnergyTypes.GetSupportedForEngine;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace ApplicationTests.VehicleEnergyTypes.GetSupportedForEngine;

public class GetSupportedEnergyTypeForEngineQueryValidatorTests
{
    private readonly GetSupportedEnergyTypeForEngineQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenEngineTypeIsValid_ShouldNotHaveError()
    {
        var query = new GetSupportedEnergyTypeForEngineQuery(EngineType.Electric);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(q => q.EngineType);
    }

    [Fact]
    public void Validate_WhenEngineTypeIsInvalid_ShouldHaveError()
    {
        const EngineType requestedEngineType = (EngineType)999;
        var query = new GetSupportedEnergyTypeForEngineQuery(requestedEngineType);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrors();
    }
}