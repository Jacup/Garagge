using Domain.Users;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Tests.Users;
public class UserErrorTests
{
    [Fact]
    public void NotFound_UserIdProvided_ReturnsNotFoundErrorWithCorrectDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var error = UserErrors.NotFound(userId);

        // Assert
        error.Code.ShouldBe("Users.NotFound");
        error.Description.ShouldBe($"The user with the Id = '{userId}' was not found");
        error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public void Unauthorized_AccessDenied_ReturnsUnauthorizedError()
    {
        // Act
        var error = UserErrors.Unauthorized;

        // Assert
        error.Code.ShouldBe("Users.Unauthorized");
        error.Description.ShouldBe("You are not authorized to perform this action.");
        error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public void WrongPassword_InvalidPasswordProvided_ReturnsUnauthorizedError()
    {
        // Act
        var error = UserErrors.WrongPassword;

        // Assert
        error.Code.ShouldBe("Users.WrongPassword");
        error.Description.ShouldBe("The provided password is incorrect");
        error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public void NotFoundByEmail_EmailNotInSystem_ReturnsNotFoundError()
    {
        // Act
        var error = UserErrors.NotFoundByEmail;

        // Assert
        error.Code.ShouldBe("Users.NotFoundByEmail");
        error.Description.ShouldBe("The user with the specified email was not found");
        error.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public void EmailNotUnique_EmailAlreadyExists_ReturnsConflictError()
    {
        // Act
        var error = UserErrors.EmailNotUnique;

        // Assert
        error.Code.ShouldBe("Users.EmailNotUnique");
        error.Description.ShouldBe("The provided email is not unique");
        error.Type.ShouldBe(ErrorType.Conflict);
    }
}
