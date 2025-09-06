using Application.EnergyEntries.Delete;
using FluentValidation.TestHelper;

namespace ApplicationTests.EnergyEntries.DeleteEnergyEntry;

public class DeleteEnergyEntryCommandValidatorTests
{
    private readonly DeleteEnergyEntryCommandValidator _validator;

    public DeleteEnergyEntryCommandValidatorTests()
    {
        _validator = new DeleteEnergyEntryCommandValidator();
    }

    [Fact]
    public void Id_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteEnergyEntryCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Energy entry ID is required.");
    }

    [Fact]
    public void VehicleId_IsEmpty_HasValidationError()
    {
        // Arrange
        var command = new DeleteEnergyEntryCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VehicleId)
            .WithErrorMessage("Vehicle ID is required.");
    }

    [Fact]
    public void AllFields_AreValid_PassesValidation()
    {
        // Arrange
        var command = new DeleteEnergyEntryCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void AllFields_AreInvalid_HasAllValidationErrors()
    {
        // Arrange
        var command = new DeleteEnergyEntryCommand(Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.VehicleId);
        result.Errors.Count.ShouldBe(2);
    }
}