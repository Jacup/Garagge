using Application.ServiceRecords.Get;
using FluentValidation.TestHelper;

namespace ApplicationTests.ServiceRecords.Get;

public class GetServiceRecordsQueryValidatorTests
{
    private readonly GetServiceRecordsQueryValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WhenPageIsZeroOrNegative_ShouldHaveError(int page)
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), page, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.Page)
            .WithErrorMessage("Page must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Validate_WhenPageIsPositive_ShouldNotHaveError(int page)
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), page, 10);

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
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.PageSize)
            .WithErrorMessage("PageSize must be between 1 and 100");
    }

    [Fact]
    public void Validate_WhenPageSizeExceedsMaximum_ShouldHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 101);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.PageSize)
            .WithErrorMessage("PageSize must be between 1 and 100");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WhenPageSizeIsValid_ShouldNotHaveError(int pageSize)
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, pageSize);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validate_WhenDateFromIsAfterDateTo_ShouldHaveError()
    {
        // Arrange
        var dateFrom = new DateTime(2024, 1, 15);
        var dateTo = new DateTime(2024, 1, 10);
        var query = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            DateFrom: dateFrom, 
            DateTo: dateTo);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.DateFrom)
            .WithErrorMessage("DateFrom must be before or equal to DateTo");
    }

    [Fact]
    public void Validate_WhenDateFromIsBeforeDateTo_ShouldNotHaveError()
    {
        // Arrange
        var dateFrom = new DateTime(2024, 1, 10);
        var dateTo = new DateTime(2024, 1, 15);
        var query = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            DateFrom: dateFrom, 
            DateTo: dateTo);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.DateFrom);
    }

    [Fact]
    public void Validate_WhenDateFromEqualsDateTo_ShouldNotHaveError()
    {
        // Arrange
        var date = new DateTime(2024, 1, 10);
        var query = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            DateFrom: date, 
            DateTo: date);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.DateFrom);
    }

    [Fact]
    public void Validate_WhenOnlyDateFromIsProvided_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            DateFrom: new DateTime(2024, 1, 10));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.DateFrom);
    }

    [Fact]
    public void Validate_WhenOnlyDateToIsProvided_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            DateTo: new DateTime(2024, 1, 10));

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.DateFrom);
    }

    [Fact]
    public void Validate_WhenNeitherDateIsProvided_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.DateFrom);
        result.ShouldNotHaveValidationErrorFor(q => q.DateTo);
    }

    [Theory]
    [InlineData("servicedate")]
    [InlineData("totalcost")]
    [InlineData("mileage")]
    [InlineData("title")]
    [InlineData("SERVICEDATE")]
    [InlineData("TotalCost")]
    [InlineData("Mileage")]
    [InlineData("TITLE")]
    public void Validate_WhenSortByIsValid_ShouldNotHaveError(string sortBy)
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SortBy: sortBy);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SortBy);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("cost")]
    [InlineData("date")]
    [InlineData("name")]
    [InlineData("price")]
    public void Validate_WhenSortByIsInvalid_ShouldHaveError(string sortBy)
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SortBy: sortBy);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.SortBy)
            .WithErrorMessage("Invalid sortBy value. Allowed values: servicedate, totalcost, mileage, title");
    }

    [Fact]
    public void Validate_WhenSortByIsNull_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SortBy: null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SortBy);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WhenSortDescendingIsAnyValue_ShouldNotHaveError(bool sortDescending)
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SortDescending: sortDescending);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenSearchTermIsNull_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenSearchTermIsProvided_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "oil change");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.SearchTerm);
    }

    [Fact]
    public void Validate_WhenServiceTypeIdIsNull_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, ServiceTypeId: null);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.ServiceTypeId);
    }

    [Fact]
    public void Validate_WhenServiceTypeIdIsProvided_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, ServiceTypeId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.ServiceTypeId);
    }

    [Fact]
    public void Validate_WhenAllParametersAreValid_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var query = new GetServiceRecordsQuery(
            VehicleId: Guid.NewGuid(),
            Page: 1,
            PageSize: 20,
            SearchTerm: "oil",
            ServiceTypeId: Guid.NewGuid(),
            DateFrom: new DateTime(2024, 1, 1),
            DateTo: new DateTime(2024, 12, 31),
            SortBy: "servicedate",
            SortDescending: true);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

