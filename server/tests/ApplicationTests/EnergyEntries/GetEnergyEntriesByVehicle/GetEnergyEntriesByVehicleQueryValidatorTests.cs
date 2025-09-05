using Application.EnergyEntries.GetEnergyEntriesByVehicle;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace ApplicationTests.EnergyEntries.GetEnergyEntriesByVehicle;

public class GetEnergyEntriesByVehicleQueryValidatorTests
{
    private readonly GetEnergyEntriesByVehicleQueryValidator _validator;

    public GetEnergyEntriesByVehicleQueryValidatorTests()
    {
        _validator = new GetEnergyEntriesByVehicleQueryValidator();
    }

    [Fact]
    public void VehicleId_IsEmpty_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { VehicleId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("Vehicle ID is required.");
    }

    [Fact]
    public void VehicleId_IsValid_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VehicleId);
    }

    [Fact]
    public void Page_IsZero_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { Page = 0 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page must be greater than 0.");
    }

    [Fact]
    public void Page_IsNegative_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { Page = -1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage("Page must be greater than 0.");
    }

    [Fact]
    public void Page_IsPositive_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery() with { Page = 1 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void PageSize_IsZero_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 0 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Fact]
    public void PageSize_IsGreaterThan100_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 101 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Fact]
    public void PageSize_IsWithinValidRange_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 20 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void EnergyType_IsNull_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery() with { EnergyType = null };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyType);
    }

    [Fact]
    public void EnergyType_IsValid_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery() with { EnergyType = EnergyType.Electric };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyType);
    }

    [Fact]
    public void EnergyType_IsInvalid_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { EnergyType = (EnergyType)999 };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EnergyType)
            .WithErrorMessage("Invalid energy type.");
    }

    [Fact]
    public void AllFields_AreValid_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static GetEnergyEntriesByVehicleQuery CreateValidQuery()
    {
        return new GetEnergyEntriesByVehicleQuery(
            VehicleId: Guid.NewGuid(),
            Page: 1,
            PageSize: 20,
            EnergyType: EnergyType.Gasoline);
    }
}
