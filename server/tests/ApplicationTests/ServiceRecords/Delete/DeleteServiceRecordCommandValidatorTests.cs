using Application.ServiceRecords.Delete;
using FluentValidation.TestHelper;

namespace ApplicationTests.ServiceRecords.Delete;

public class DeleteServiceRecordCommandValidatorTests
{
    private readonly DeleteServiceRecordCommandValidator _validator;

    public DeleteServiceRecordCommandValidatorTests()
    {
        _validator = new DeleteServiceRecordCommandValidator();
    }

    [Fact]
    public void ServiceRecordId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteServiceRecordCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId)
            .WithErrorMessage("ServiceRecordId is required.");
    }

    [Fact]
    public void VehicleId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteServiceRecordCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("VehicleId is required.");
    }

    [Fact]
    public void AllFields_AreValid_PassesValidation()
    {
        // Arrange
        var command = new DeleteServiceRecordCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void AllFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var command = new DeleteServiceRecordCommand(Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId);
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
        result.Errors.Count.ShouldBe(2);
    }
}