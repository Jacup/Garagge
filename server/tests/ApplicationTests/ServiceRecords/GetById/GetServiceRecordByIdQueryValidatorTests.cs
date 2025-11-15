using Application.ServiceRecords.GetById;
using FluentValidation.TestHelper;

namespace ApplicationTests.ServiceRecords.GetById;

public class GetServiceRecordByIdQueryValidatorTests
{
    private readonly GetServiceRecordByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_ShouldHaveError()
    {
        // Arrange
        var query = new GetServiceRecordByIdQuery(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.VehicleId)
            .WithErrorMessage("VehicleId is required.");
    }

    [Fact]
    public void Validate_WhenServiceRecordIdIsEmpty_ShouldHaveError()
    {
        // Arrange
        var query = new GetServiceRecordByIdQuery(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(q => q.ServiceRecordId)
            .WithErrorMessage("ServiceRecordId is required.");
    }

    [Fact]
    public void Validate_WhenBothIdsAreEmpty_ShouldHaveMultipleErrors()
    {
        // Arrange
        var query = new GetServiceRecordByIdQuery(Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.VehicleId);
        result.ShouldHaveValidationErrorFor(q => q.ServiceRecordId);
    }

    [Fact]
    public void Validate_WhenBothIdsAreValid_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new GetServiceRecordByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}