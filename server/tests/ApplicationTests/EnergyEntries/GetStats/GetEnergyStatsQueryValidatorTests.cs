using Application.EnergyEntries.GetStats;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace ApplicationTests.EnergyEntries.GetStats;

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
        var query = new GetEnergyStatsQuery(Guid.Empty, [EnergyType.Gasoline]);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("Vehicle ID is required.");
    }

    [Fact]
    public void EnergyTypes_IsEmpty_PassesValidation()
    {
        // Arrange
        var query = new GetEnergyStatsQuery(Guid.NewGuid(), []);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyTypes);
    }
    
    [Fact]
    public void EnergyTypes_MultipleValues_PassesValidation()
    {
        // Arrange
        var query = new GetEnergyStatsQuery(Guid.NewGuid(), [EnergyType.Gasoline, EnergyType.Electric]);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyTypes);
    }
    [Fact]
    public void Request_IsValid_PassesValidation()
    {
        // Arrange
        var query = new GetEnergyStatsQuery(Guid.NewGuid(), [EnergyType.Gasoline]);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VehicleId);
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyTypes);
    }

}
