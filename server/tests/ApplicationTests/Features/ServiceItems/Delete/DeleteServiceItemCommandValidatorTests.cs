using Application.Features.ServiceItems;
using Application.Features.ServiceItems.Delete;
using FluentValidation.TestHelper;

namespace ApplicationTests.Features.ServiceItems.Delete;

public class DeleteServiceItemCommandValidatorTests
{
    private readonly DeleteServiceItemCommandValidator _validator = new();

    [Fact]
    public void ServiceItemId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteServiceItemCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceItemId)
            .WithErrorCode(ServiceItemsErrors.IdRequired.Code);
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
            .WithErrorCode(ServiceItemsErrors.ServiceRecordIdRequired.Code);
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
            .WithErrorCode(ServiceItemsErrors.VehicleIdRequired.Code);
    }

    [Fact]
    public void AllFields_AreValid_PassesValidation()
    {
        // Arrange
        var command = new DeleteServiceItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AllFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var command = new DeleteServiceItemCommand(Guid.Empty, Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceItemId)
            .WithErrorCode(ServiceItemsErrors.IdRequired.Code);

        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId)
            .WithErrorCode(ServiceItemsErrors.ServiceRecordIdRequired.Code);

        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorCode(ServiceItemsErrors.VehicleIdRequired.Code);

        result.Errors.Count.ShouldBe(3);
    }
}