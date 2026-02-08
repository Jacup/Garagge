using Application.Auth;
using Application.Users;
using Application.Users.Me.Update;
using FluentValidation.TestHelper;

namespace ApplicationTests.Users.Me.Update;

public class UpdateMeCommandValidatorTests
{
    private readonly UpdateMeCommandValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyFirstName_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var command = new UpdateMeCommand("test@example.com", firstName, "Doe");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required.");
    }

    [Fact]
    public void Validate_NullFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateMeCommand("test@example.com", null!, "Doe");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyLastName_ShouldHaveValidationError(string lastName)
    {
        // Arrange
        var command = new UpdateMeCommand("test@example.com", "John", lastName);

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public void Validate_NullLastName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateMeCommand("test@example.com", "John", null!);

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_MissingEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new UpdateMeCommand(email, "John", "Doe");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(UserErrors.EmailRequired.Description);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new UpdateMeCommand(email, "John", "Doe");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(UserErrors.EmailInvalid.Description);
    }

    [Fact]
    public void Validate_NullEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateMeCommand(null!, "John", "Doe");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateMeCommand("test@example.com", "John", "Doe");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidEmailFormats_ShouldNotHaveValidationErrors()
    {
        var validEmails = new[] { "test@example.com", "user.name@domain.co.uk", "firstname+lastname@company.org" };

        foreach (var email in validEmails)
        {
            var command = new UpdateMeCommand(email, "John", "Doe");
            var result = _sut.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }
    }
}