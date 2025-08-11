using Application.Vehicles.GetMyVehicles;
using FluentValidation.TestHelper;

namespace ApplicationTests.Vehicles.GetMyVehicles;

public class GetMyVehiclesQueryValidatorTests
{
    private readonly GetMyVehiclesQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ShouldHaveError()
    {
        // Arrange
        var query = new GetMyVehiclesQuery(Guid.Empty, 1, 10, null);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.UserId)
            .WithErrorMessage("User ID cannot be empty.");
    }

    [Fact]
    public void Validate_WhenUserIdIsValid_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 10, null);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WhenPageIsZeroOrNegative_ShouldHaveError(int page)
    {
        // Arrange
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), page, 10, null);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), page, 10, null);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, pageSize, null);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 101, null);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, pageSize, null);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validate_WhenSearchTermIsNull_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 10, null);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenSearchTermIsEmpty_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 10, string.Empty);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 10, searchTerm);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 10, longSearchTerm);
        
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
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 10, maxLengthSearchTerm);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenAllParametersAreValid_ShouldPassValidation()
    {
        // Arrange
        var query = new GetMyVehiclesQuery(Guid.NewGuid(), 1, 20, "Audi");
        
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
        var query = new GetMyVehiclesQuery(Guid.Empty, 0, -5, longSearchTerm);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(q => q.UserId);
        result.ShouldHaveValidationErrorFor(q => q.Page);
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
        result.ShouldHaveValidationErrorFor(q => q.SearchTerm);
        result.IsValid.ShouldBeFalse();
    }
}
