using Application.Auth;
using Application.Auth.ChangePassword;
using FluentValidation.TestHelper;

namespace ApplicationTests.Auth.ChangePassword;

public class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyCurrentPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ChangePasswordCommand("", "newPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorCode(AuthErrors.InvalidPassword(8).Code);
    }

    [Fact]
    public void Validate_CurrentPasswordTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ChangePasswordCommand("short", "newPassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorCode(AuthErrors.InvalidPassword(8).Code);
    }

    [Fact]
    public void Validate_EmptyNewPassword_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ChangePasswordCommand("currentPassword123", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorCode(AuthErrors.InvalidPassword(8).Code);
    }

    [Fact]
    public void Validate_NewPasswordTooShort_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ChangePasswordCommand("currentPassword123", "short");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorCode(AuthErrors.InvalidPassword(8).Code);
    }

    [Fact]
    public void Validate_NewPasswordSameAsCurrentPassword_ShouldHaveValidationError()
    {
        // Arrange
        const string password = "samePassword123";
        var command = new ChangePasswordCommand(password, password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorCode(AuthErrors.NewPasswordSameAsOld.Code);
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new ChangePasswordCommand("currentPassword123", "newPassword456");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("password1", "password2")]
    [InlineData("validPassword123", "anotherValidPassword456")]
    [InlineData("12345678", "87654321")]
    public void Validate_ValidPasswords_ShouldNotHaveValidationErrors(string currentPassword, string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(currentPassword, newPassword);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
