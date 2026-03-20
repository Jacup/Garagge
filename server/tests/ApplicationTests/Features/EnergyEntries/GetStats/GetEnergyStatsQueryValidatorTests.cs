using Application.Features.EnergyEntries.GetStats;
using Domain.Enums.Energy;
using FluentValidation.TestHelper;

namespace ApplicationTests.Features.EnergyEntries.GetStats;

public class GetEnergyStatsQueryValidatorTests
{
    private readonly GetEnergyStatsQueryValidator _validator;

    public GetEnergyStatsQueryValidatorTests()
    {
        _validator = new GetEnergyStatsQueryValidator();
    }

    [Fact]
    public void VehicleId_IsEmpty_HasValidationError()
    {
        // Arrange
        var query = new GetEnergyStatsQuery(Guid.Empty, StatsPeriod.Lifetime);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("Vehicle ID is required.");
    }

    [Fact]
    public void Period_IsInvalidEnumValue_HasValidationError()
    {
        // Arrange
        var query = new GetEnergyStatsQuery(Guid.NewGuid(), (StatsPeriod)999);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Period);
    }

    [Fact]
    public void Period_IsValidEnumValue_PassesValidation()
    {
        // Arrange
        var query = new GetEnergyStatsQuery(Guid.NewGuid(), StatsPeriod.Month);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Period);
    }
}