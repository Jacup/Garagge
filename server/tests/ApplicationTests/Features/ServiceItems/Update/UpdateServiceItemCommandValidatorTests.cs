using Application.Features.ServiceItems;
using Application.Features.ServiceItems.Update;
using Domain.Enums.Services;
using FluentValidation.TestHelper;

namespace ApplicationTests.Features.ServiceItems.Update;

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
            .WithErrorCode(ServiceItemsErrors.IdRequired.Code);
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
            .WithErrorCode(ServiceItemsErrors.ServiceRecordIdRequired.Code);
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
            .WithErrorCode(ServiceItemsErrors.NameRequired.Code);
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
            .WithErrorCode(ServiceItemsErrors.NameTooLong(128).Code);
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
            .WithErrorCode(ServiceItemsErrors.InvalidType.Code);
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
            .WithErrorCode(ServiceItemsErrors.UnitPriceNegative.Code);
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
            .WithErrorCode(ServiceItemsErrors.QuantityInvalid.Code);
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
            .WithErrorCode(ServiceItemsErrors.QuantityInvalid.Code);
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
            .WithErrorCode(ServiceItemsErrors.PartNumberTooLong(64).Code);
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
            .WithErrorCode(ServiceItemsErrors.NotesTooLong(500).Code);
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
        result.ShouldNotHaveAnyValidationErrors();
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
        result.ShouldNotHaveAnyValidationErrors();
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
        // Tutaj również sprawdzamy konkretne kody błędów
        result.ShouldHaveValidationErrorFor(x => x.ServiceItemId)
            .WithErrorCode(ServiceItemsErrors.IdRequired.Code);

        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId)
            .WithErrorCode(ServiceItemsErrors.ServiceRecordIdRequired.Code);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(ServiceItemsErrors.NameRequired.Code);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorCode(ServiceItemsErrors.InvalidType.Code);

        result.ShouldHaveValidationErrorFor(x => x.UnitPrice)
            .WithErrorCode(ServiceItemsErrors.UnitPriceNegative.Code);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorCode(ServiceItemsErrors.QuantityInvalid.Code);
    }
}