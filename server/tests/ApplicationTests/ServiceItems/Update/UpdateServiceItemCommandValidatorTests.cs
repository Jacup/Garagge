using Application.ServiceItems.Update;
using Domain.Enums.Services;
using FluentValidation.TestHelper;

namespace ApplicationTests.ServiceItems.Update;

public class UpdateServiceItemCommandValidatorTests
{
    private readonly UpdateServiceItemCommandValidator _validator;

    public UpdateServiceItemCommandValidatorTests()
    {
        _validator = new UpdateServiceItemCommandValidator();
    }

    [Fact]
    public void ServiceItemId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.Empty,
            Guid.NewGuid(),
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

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
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.Empty,
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId)
            .WithErrorMessage("ServiceRecordId is required.");
    }

    [Fact]
    public void Name_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Name_ExceedsMaxLength_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('a', 129),
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 128 characters.");
    }

    [Fact]
    public void Type_IsInvalidEnum_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Item",
            (ServiceItemType)999,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Type must be a valid ServiceItemType.");
    }

    [Fact]
    public void UnitPrice_IsNegative_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Item",
            ServiceItemType.Part,
            -10.00m,
            1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
            .WithErrorMessage("Unit price cannot be negative.");
    }

    [Fact]
    public void Quantity_IsZero_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            0,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void Quantity_IsNegative_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            -1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void PartNumber_ExceedsMaxLength_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            new string('a', 65),
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PartNumber)
            .WithErrorMessage("Part number cannot exceed 64 characters.");
    }

    [Fact]
    public void Notes_ExceedsMaxLength_HasValidationError()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            new string('a', 501));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 500 characters.");
    }

    [Fact]
    public void AllFields_AreValid_PassesValidation()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Engine Oil 5W-30",
            ServiceItemType.Part,
            45.00m,
            5,
            "12345",
            "Premium synthetic oil");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void AllFields_AreValidWithMinimalData_PassesValidation()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Labor",
            ServiceItemType.Labor,
            50.00m,
            1,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void MultipleFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var command = new UpdateServiceItemCommand(
            Guid.Empty,
            Guid.Empty,
            "",
            (ServiceItemType)999,
            -10.00m,
            0,
            null,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceItemId);
        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.Errors.Count.ShouldBe(6);
    }
}