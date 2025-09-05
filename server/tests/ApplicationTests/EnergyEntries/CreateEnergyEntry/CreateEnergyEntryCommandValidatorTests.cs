using Application.Abstractions;
using Application.EnergyEntries.CreateEnergyEntry;
using Domain.Enums;
using FluentValidation.TestHelper;
using Moq;

namespace ApplicationTests.EnergyEntries.CreateEnergyEntry;

public class CreateEnergyEntryCommandValidatorTests
{
    private readonly CreateEnergyEntryCommandValidator _validator;
    private readonly DateTime _currentDateTime = new(2023, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    public CreateEnergyEntryCommandValidatorTests()
    {
        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_currentDateTime);
        _validator = new CreateEnergyEntryCommandValidator(dateTimeProviderMock.Object);
    }

    [Fact]
    public void VehicleId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { VehicleId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
    }

    [Fact]
    public void VehicleId_IsValid_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VehicleId);
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

    [Fact]
    public void Mileage_IsZero_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Mileage)
            .WithErrorMessage("Mileage must be greater than 0.");
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
            .WithErrorMessage("Mileage must be greater than 0.");
    }

    [Fact]
    public void Mileage_IsPositive_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = 100 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Mileage);
    }

    [Fact]
    public void Type_IsInvalid_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Type = (EnergyType)999 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Invalid energy type.");
    }

    [Fact]
    public void Type_IsValid_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Type = EnergyType.Gasoline };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void EnergyUnit_IsInvalid_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { EnergyUnit = (EnergyUnit)999 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EnergyUnit)
            .WithErrorMessage("Invalid energy unit.");
    }

    [Fact]
    public void EnergyUnit_IsValid_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { EnergyUnit = EnergyUnit.Liter };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EnergyUnit);
    }

    [Fact]
    public void Volume_IsZero_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Volume = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Volume)
            .WithErrorMessage("Volume must be greater than 0.");
    }

    [Fact]
    public void Volume_IsNegative_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Volume = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Volume)
            .WithErrorMessage("Volume must be greater than 0.");
    }

    [Fact]
    public void Volume_IsPositive_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Volume = 50.5m };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Volume);
    }

    [Fact]
    public void Cost_IsNull_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Cost = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Cost);
    }

    [Fact]
    public void Cost_IsZero_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Cost = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cost)
            .WithErrorMessage("Cost must be greater than 0.");
    }

    [Fact]
    public void Cost_IsNegative_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { Cost = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cost)
            .WithErrorMessage("Cost must be greater than 0.");
    }

    [Fact]
    public void Cost_IsPositive_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { Cost = 100.50m };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Cost);
    }

    [Fact]
    public void PricePerUnit_IsNull_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { PricePerUnit = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PricePerUnit);
    }

    [Fact]
    public void PricePerUnit_IsZero_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { PricePerUnit = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PricePerUnit)
            .WithErrorMessage("Price per unit must be greater than 0.");
    }

    [Fact]
    public void PricePerUnit_IsNegative_HasValidationError()
    {
        // Arrange
        var command = CreateValidCommand() with { PricePerUnit = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PricePerUnit)
            .WithErrorMessage("Price per unit must be greater than 0.");
    }

    [Fact]
    public void PricePerUnit_IsPositive_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand() with { PricePerUnit = 1.50m };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PricePerUnit);
    }

    private static CreateEnergyEntryCommand CreateValidCommand()
    {
        return new CreateEnergyEntryCommand(
            VehicleId: Guid.NewGuid(),
            Date: new DateOnly(2023, 10, 14),
            Mileage: 1000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.0m,
            Cost: 100.0m,
            PricePerUnit: 2.0m);
    }
}
