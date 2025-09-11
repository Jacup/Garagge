using Application.Auth;
using Application.Auth.Register;
using FluentValidation.TestHelper;

namespace ApplicationTests.Auth.Register;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _sut = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyFirstName_ShouldHaveValidationError(string firstName)
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", firstName, "Doe", "password123");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name is required.");
    }

    [Fact]
    public void Validate_NullFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", null!, "Doe", "password123");

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
        var command = new RegisterUserCommand("test@example.com", "John", lastName, "password123");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
              .WithErrorMessage("Last name is required.");
    }

    [Fact]
    public void Validate_NullLastName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "John", null!, "password123");

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
        var command = new RegisterUserCommand(email, "John", "Doe", "password123");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(AuthErrors.MissingEmail.Description);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Validate_InvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(email, "John", "Doe", "password123");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(AuthErrors.InvalidEmail.Description);
    }

    [Fact]
    public void Validate_NullEmail_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(null!, "John", "Doe", "password123");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567")] // 7 characters - too short
    public void Validate_InvalidPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "John", "Doe", password);

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [Fact]
    public void Validate_NullPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "John", "Doe", null!);

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterUserCommand("test@example.com", "John", "Doe", "password123");

        // Act & Assert
        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidEmailFormats_ShouldNotHaveValidationErrors()
    {
        // Arrange & Act & Assert
        var validEmails = new[] { "test@example.com", "user.name@domain.co.uk", "firstname+lastname@company.org" };

        foreach (var email in validEmails)
        {
            var command = new RegisterUserCommand(email, "John", "Doe", "password123");
            var result = _sut.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }
    }
}