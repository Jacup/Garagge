using Application.ServiceItems.Delete;
using FluentValidation.TestHelper;

namespace ApplicationTests.ServiceItems.Delete;

public class DeleteServiceItemCommandValidatorTests
{
    private readonly DeleteServiceItemCommandValidator _validator;

    public DeleteServiceItemCommandValidatorTests()
    {
        _validator = new DeleteServiceItemCommandValidator();
    }

    [Fact]
    public void ServiceItemId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteServiceItemCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceItemId)
            .WithErrorMessage("ServiceItemId is required.");
    }

    [Fact]
    public void ServiceRecordId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteServiceItemCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

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
        var command = new DeleteServiceItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

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
        var command = new DeleteServiceItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void AllFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var command = new DeleteServiceItemCommand(Guid.Empty, Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceItemId);
        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId);
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
        result.Errors.Count.ShouldBe(3);
    }
}

