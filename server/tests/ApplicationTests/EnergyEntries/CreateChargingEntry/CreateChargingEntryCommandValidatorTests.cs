using Application.Abstractions;
using Application.EnergyEntries.CreateChargingEntry;
using Domain.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace ApplicationTests.EnergyEntries.CreateChargingEntry;

public class CreateChargingEntryCommandValidatorTests
{
    private readonly CreateChargingEntryCommandValidator _validator;
    private readonly DateTime _currentDateTime = new(2023, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    public CreateChargingEntryCommandValidatorTests()
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_currentDateTime);
        _validator = new CreateChargingEntryCommandValidator(dateTimeProviderMock.Object);
    }

    [Fact]
    public void Date_IsInFuture_HasValidationError()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(_currentDateTime.AddDays(1));
        var command = CreateValidCommand() with { Date = futureDate };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date)
            .WithErrorMessage("Date cannot be in the future.");
    }

    [Fact]
    public void Date_IsToday_PassesValidation()
    {
        // Arrange
        var today = DateOnly.FromDateTime(_currentDateTime);
        var command = CreateValidCommand() with { Date = today };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Date_IsInPast_PassesValidation()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(_currentDateTime.AddDays(-1));
        var command = CreateValidCommand() with { Date = pastDate };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Mileage_IsNotGreaterThanZero_HasValidationError(int invalidMileage)
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = invalidMileage };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Mileage)
            .WithErrorMessage("Mileage must be greater than 0.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(50000)]
    public void Mileage_IsGreaterThanZero_PassesValidation(int validMileage)
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = validMileage };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Mileage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50.99)]
    public void Cost_IsNotGreaterThanZero_HasValidationError(decimal invalidCost)
    {
        // Arrange
        var command = CreateValidCommand() with { Cost = invalidCost };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cost)
            .WithErrorMessage("Cost must be greater than 0.");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100.50)]
    public void Cost_IsGreaterThanZero_PassesValidation(decimal validCost)
    {
        // Arrange
        var command = CreateValidCommand() with { Cost = validCost };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Cost);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-25.75)]
    public void EnergyAmount_IsNotGreaterThanZero_HasValidationError(decimal invalidEnergyAmount)
    {
        // Arrange
        var command = CreateValidCommand() with { EnergyAmount = invalidEnergyAmount };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EnergyAmount)
            .WithErrorMessage("Energy amount must be greater than zero");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(42.5)]
    public void EnergyAmount_IsGreaterThanZero_PassesValidation(decimal validEnergyAmount)
    {
        // Arrange
        var command = CreateValidCommand() with { EnergyAmount = validEnergyAmount };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5.99)]
    public void PricePerUnit_IsNotGreaterThanZero_HasValidationError(decimal invalidPricePerUnit)
    {
        // Arrange
        var command = CreateValidCommand() with { PricePerUnit = invalidPricePerUnit };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PricePerUnit)
            .WithErrorMessage("Price per unit must be greater than 0.");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(2.75)]
    public void PricePerUnit_IsGreaterThanZero_PassesValidation(decimal validPricePerUnit)
    {
        // Arrange
        var command = CreateValidCommand() with { PricePerUnit = validPricePerUnit };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PricePerUnit);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ChargingDurationMinutes_IsNotGreaterThanZero_WhenHasValue_HasValidationError(int invalidDuration)
    {
        // Arrange
        var command = CreateValidCommand() with { ChargingDurationMinutes = invalidDuration };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChargingDurationMinutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(120)]
    public void ChargingDurationMinutes_IsGreaterThanZero_WhenHasValue_PassesValidation(int validDuration)
    {
        // Arrange
        var command = CreateValidCommand() with { ChargingDurationMinutes = validDuration };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChargingDurationMinutes);
    }

    [Fact]
    public void ChargingDurationMinutes_IsNull_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { ChargingDurationMinutes = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ChargingDurationMinutes);
    }

    [Fact]
    public void Unit_IsValidEnum_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Unit = EnergyUnit.kWh };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Unit);
    }

    [Fact]
    public void AllFields_AreValid_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void AllFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(_currentDateTime.AddDays(1));
        var command = new CreateChargingEntryCommand(
            Guid.NewGuid(),
            futureDate,
            -1,
            -1,
            -1,
            -1,
            EnergyUnit.kWh,
            -1);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
        result.ShouldHaveValidationErrorFor(x => x.Mileage);
        result.ShouldHaveValidationErrorFor(x => x.Cost);
        result.ShouldHaveValidationErrorFor(x => x.EnergyAmount);
        result.ShouldHaveValidationErrorFor(x => x.PricePerUnit);
        result.ShouldHaveValidationErrorFor(x => x.ChargingDurationMinutes);
        result.Errors.Count.ShouldBe(6);
    }

    private CreateChargingEntryCommand CreateValidCommand()
    {
        return new CreateChargingEntryCommand(
            Guid.NewGuid(),
            DateOnly.FromDateTime(_currentDateTime),
            50000,
            85.30m,
            42.5m,
            2.01m,
            EnergyUnit.kWh,
            90);
    }
}