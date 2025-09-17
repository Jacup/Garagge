using Application.Vehicles.GetVehicles;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles.GetVehicles;

public class GetVehiclesQueryValidatorTests
{
    private readonly GetVehiclesQueryValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WhenPageIsZeroOrNegative_ShouldHaveError(int page)
    {
        // Arrange
        var query = new GetVehiclesQuery(10, page, null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.Page)
            .WithErrorMessage("Page must be greater than 0.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Validate_WhenPageIsPositive_ShouldNotHaveError(int page)
    {
        // Arrange
        var query = new GetVehiclesQuery(10, page, null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WhenPageSizeIsZeroOrNegative_ShouldHaveError(int pageSize)
    {
        // Arrange
        var query = new GetVehiclesQuery(pageSize, 1, null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Fact]
    public void Validate_WhenPageSizeExceedsMaximum_ShouldHaveError()
    {
        // Arrange
        var query = new GetVehiclesQuery(101, 1, null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.PageSize)
            .WithErrorMessage("Page size must be between 1 and 100.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WhenPageSizeIsValid_ShouldNotHaveError(int pageSize)
    {
        // Arrange
        var query = new GetVehiclesQuery(pageSize, 1, null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validate_WhenSearchTermIsNull_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetVehiclesQuery(10, 1, null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenSearchTermIsEmpty_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetVehiclesQuery(10, 1, string.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Audi")]
    [InlineData("BMW X5")]
    [InlineData("ThisIsValidSearchTermWith32Chars")]
    public void Validate_WhenSearchTermIsValid_ShouldNotHaveError(string searchTerm)
    {
        // Arrange
        var query = new GetVehiclesQuery(10, 1, searchTerm);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenSearchTermExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var longSearchTerm = new string('A', 33);
        var query = new GetVehiclesQuery(10, 1, longSearchTerm);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.SearchTerm)
            .WithErrorMessage("Search term cannot exceed 32 characters.");
    }

    [Fact]
    public void Validate_WhenSearchTermIsExactlyMaxLength_ShouldNotHaveError()
    {
        // Arrange
        var maxLengthSearchTerm = new string('A', 32);
        var query = new GetVehiclesQuery(10, 1, maxLengthSearchTerm);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenAllParametersAreValid_ShouldPassValidation()
    {
        // Arrange
        var query = new GetVehiclesQuery(20, 1, "Audi");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WhenMultipleParametersAreInvalid_ShouldHaveMultipleErrors()
    {
        // Arrange
        var longSearchTerm = new string('X', 33);
        var query = new GetVehiclesQuery(-5, 0, longSearchTerm);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.Page);
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
        result.ShouldHaveValidationErrorFor(q => q.SearchTerm);
        result.IsValid.ShouldBeFalse();
    }
}