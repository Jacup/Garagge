using Application.Abstractions;
using Application.ServiceRecords.Update;
using FluentValidation.TestHelper;
using Moq;

namespace ApplicationTests.ServiceRecords.Update;

public class UpdateServiceRecordCommandValidatorTests
{
    private readonly UpdateServiceRecordCommandValidator _validator;
    private readonly DateTime _currentDateTime = new(2024, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    public UpdateServiceRecordCommandValidatorTests()
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_currentDateTime);
        _validator = new UpdateServiceRecordCommandValidator(dateTimeProviderMock.Object);
    }

    [Fact]
    public void ServiceRecordId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { ServiceRecordId = Guid.Empty };

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
        var command = CreateValidCommand() with { VehicleId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("VehicleId is required.");
    }

    [Fact]
    public void ServiceTypeId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { ServiceTypeId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceTypeId)
            .WithErrorMessage("ServiceTypeId is required.");
    }

    [Fact]
    public void Title_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Title = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Title_IsNull_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Title = null! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Title_ExceedsMaxLength_HasValidationError()
    {
        // Arrange
        var longTitle = new string('A', 65);
        var command = CreateValidCommand() with { Title = longTitle };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 64 characters.");
    }

    [Fact]
    public void Title_IsAtMaxLength_PassesValidation()
    {
        // Arrange
        var maxLengthTitle = new string('A', 64);
        var command = CreateValidCommand() with { Title = maxLengthTitle };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Notes_ExceedsMaxLength_HasValidationError()
    {
        // Arrange
        var longNotes = new string('A', 501);
        var command = CreateValidCommand() with { Notes = longNotes };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage("Notes cannot exceed 500 characters.");
    }

    [Fact]
    public void Notes_IsAtMaxLength_PassesValidation()
    {
        // Arrange
        var maxLengthNotes = new string('A', 500);
        var command = CreateValidCommand() with { Notes = maxLengthNotes };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Notes_IsNull_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Notes = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Mileage_IsNegative_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Mileage)
            .WithErrorMessage("Mileage cannot be negative.");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    public void Mileage_IsNegative_HasValidationError_Theory(int mileage)
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = mileage };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Mileage)
            .WithErrorMessage("Mileage cannot be negative.");
    }

    [Fact]
    public void Mileage_IsZero_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Mileage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(50000)]
    public void Mileage_IsValid_PassesValidation(int mileage)
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = mileage };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Mileage);
    }

    [Fact]
    public void Mileage_IsNull_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Mileage);
    }

    [Fact]
    public void ServiceDate_IsInFuture_HasValidationError()
    {
        // Arrange
        var futureDate = _currentDateTime.AddDays(1);
        var command = CreateValidCommand() with { ServiceDate = futureDate };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceDate)
            .WithErrorMessage("Service date cannot be in the future.");
    }

    [Fact]
    public void ServiceDate_IsToday_PassesValidation()
    {
        // Arrange
        var today = _currentDateTime.Date;
        var command = CreateValidCommand() with { ServiceDate = today };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ServiceDate);
    }

    [Fact]
    public void ServiceDate_IsInPast_PassesValidation()
    {
        // Arrange
        var pastDate = _currentDateTime.AddDays(-1);
        var command = CreateValidCommand() with { ServiceDate = pastDate };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ServiceDate);
    }

    [Fact]
    public void ServiceDate_IsDefault_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { ServiceDate = default };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceDate);
    }

    [Fact]
    public void ManualCost_IsNegative_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { ManualCost = -0.01m };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ManualCost)
            .WithErrorMessage("Manual cost cannot be negative.");
    }

    [Theory]
    [InlineData(-100.50)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void ManualCost_IsNegative_HasValidationError_Theory(decimal cost)
    {
        // Arrange
        var command = CreateValidCommand() with { ManualCost = cost };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ManualCost)
            .WithErrorMessage("Manual cost cannot be negative.");
    }

    [Fact]
    public void ManualCost_IsZero_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { ManualCost = 0m };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ManualCost);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10.50)]
    [InlineData(1000)]
    public void ManualCost_IsValid_PassesValidation(decimal cost)
    {
        // Arrange
        var command = CreateValidCommand() with { ManualCost = cost };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ManualCost);
    }

    [Fact]
    public void ManualCost_IsNull_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { ManualCost = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ManualCost);
    }

    [Fact]
    public void ValidCommand_PassesAllValidations()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CommandWithMinimalData_PassesValidation()
    {
        // Arrange
        var command = new UpdateServiceRecordCommand(
            Guid.NewGuid(),
            "Basic service",
            null,
            null,
            _currentDateTime.Date,
            null,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MultipleFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var command = new UpdateServiceRecordCommand(
            Guid.Empty,
            "",
            new string('A', 501),
            -1,
            _currentDateTime.AddDays(1),
            -10m,
            Guid.Empty,
            Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ServiceRecordId);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
        result.ShouldHaveValidationErrorFor(x => x.Mileage);
        result.ShouldHaveValidationErrorFor(x => x.ServiceDate);
        result.ShouldHaveValidationErrorFor(x => x.ManualCost);
        result.ShouldHaveValidationErrorFor(x => x.ServiceTypeId);
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
        result.Errors.Count.ShouldBe(8);
    }

    private UpdateServiceRecordCommand CreateValidCommand()
    {
        return new UpdateServiceRecordCommand(
            Guid.NewGuid(),
            "Oil Change",
            "Regular maintenance",
            15000,
            _currentDateTime.Date,
            150.00m,
            Guid.NewGuid(),
            Guid.NewGuid());
    }
}