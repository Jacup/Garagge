using Application.EnergyEntries.GetByUser;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace ApplicationTests.EnergyEntries.GetEnergyEntriesByUser;

public class GetEnergyEntriesByUserQueryValidatorTests
{
    private readonly GetEnergyEntriesByUserQueryValidator _validator;

    public GetEnergyEntriesByUserQueryValidatorTests()
    {
        _validator = new GetEnergyEntriesByUserQueryValidator();
    }

    [Fact]
    public void UserId_IsEmpty_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { UserId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void UserId_IsValid_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
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
        var query = CreateValidQuery() with { EnergyTypes = null };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyTypes);
    }

    [Fact]
    public void EnergyType_IsValid_PassesValidation()
    {
        // Arrange
        var query = CreateValidQuery() with { EnergyTypes = [EnergyType.Gasoline] };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyTypes);
    }

    [Fact]
    public void EnergyType_IsInvalid_HasValidationError()
    {
        // Arrange
        var query = CreateValidQuery() with { EnergyTypes = [(EnergyType)999] };

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EnergyTypes)
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

    private static GetEnergyEntriesByUserQuery CreateValidQuery()
    {
        return new GetEnergyEntriesByUserQuery(
            UserId: Guid.NewGuid(),
            Page: 1,
            PageSize: 20,
            EnergyTypes: [EnergyType.Gasoline]);
    }
}
