using Api.Infrastructure;
using Application.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApiTests.Infrastructure;

public class CustomResultsTests
{
    [Fact]
    public void Problem_WithValidationError_ReturnsCorrectProblemDetails()
    {
        // Arrange
        var errors = new[]
        {
            Error.Failure("Field1.Required", "Field1 is required"),
            Error.Failure("Field2.Invalid", "Field2 is invalid")
        };
        var validationError = new ValidationError(errors);
        var result = Result.Failure(validationError);

        // Act
        var problemResult = CustomResults.Problem(result);

        // Assert
        problemResult.ShouldBeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)problemResult;
        
        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Title.ShouldBe("Validation.General");
        problem.ProblemDetails.Detail.ShouldBe("One or more validation errors occurred");
        problem.ProblemDetails.Extensions.ShouldContainKey("errors");
        problem.ProblemDetails.Extensions["errors"].ShouldBe(errors);
    }

    [Fact]
    public void Problem_WithNotFoundError_ReturnsNotFoundStatusCode()
    {
        // Arrange
        var error = Error.NotFound("Vehicle.NotFound", "Vehicle with id 123 was not found");
        var result = Result.Failure(error);

        // Act
        var problemResult = CustomResults.Problem(result);

        // Assert
        problemResult.ShouldBeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)problemResult;
        
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Title.ShouldBe("Vehicle.NotFound");
        problem.ProblemDetails.Detail.ShouldBe("Vehicle with id 123 was not found");
        problem.ProblemDetails.Extensions.ShouldBeEmpty();
    }

    [Fact]
    public void Problem_WithConflictError_ReturnsConflictStatusCode()
    {
        // Arrange
        var error = Error.Conflict("Vehicle.AlreadyExists", "Vehicle already exists");
        var result = Result.Failure(error);

        // Act
        var problemResult = CustomResults.Problem(result);

        // Assert
        problemResult.ShouldBeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)problemResult;
        
        problem.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        problem.ProblemDetails.Title.ShouldBe("Vehicle.AlreadyExists");
        problem.ProblemDetails.Detail.ShouldBe("Vehicle already exists");
        problem.ProblemDetails.Extensions.ShouldBeEmpty();
    }

    [Fact]
    public void Problem_WithUnauthorizedError_ReturnsUnauthorizedStatusCode()
    {
        // Arrange
        var error = Error.Unauthorized("Auth.InvalidToken", "Token is invalid");
        var result = Result.Failure(error);

        // Act
        var problemResult = CustomResults.Problem(result);

        // Assert
        problemResult.ShouldBeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)problemResult;
        
        problem.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        problem.ProblemDetails.Title.ShouldBe("Auth.InvalidToken");
        problem.ProblemDetails.Detail.ShouldBe("Token is invalid");
        problem.ProblemDetails.Extensions.ShouldBeEmpty();
    }

    [Fact]
    public void Problem_WithProblemError_ReturnsBadRequestStatusCode()
    {
        // Arrange
        var error = Error.Problem("Business.RuleViolation", "Business rule violated");
        var result = Result.Failure(error);

        // Act
        var problemResult = CustomResults.Problem(result);

        // Assert
        problemResult.ShouldBeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)problemResult;
        
        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Title.ShouldBe("Business.RuleViolation");
        problem.ProblemDetails.Detail.ShouldBe("Business rule violated");
        problem.ProblemDetails.Extensions.ShouldBeEmpty();
    }

    [Fact]
    public void Problem_WithUnknownErrorType_ReturnsInternalServerError()
    {
        // Arrange
        var error = Error.Failure("Unknown.Error", "Something went wrong");
        var result = Result.Failure(error);

        // Act
        var problemResult = CustomResults.Problem(result);

        // Assert
        problemResult.ShouldBeOfType<ProblemHttpResult>();
        var problem = (ProblemHttpResult)problemResult;
        
        problem.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        problem.ProblemDetails.Title.ShouldBe("Server failure");
        problem.ProblemDetails.Detail.ShouldBe("An unexpected error occurred");
        problem.ProblemDetails.Extensions.ShouldBeEmpty();
    }

    [Fact]
    public void Problem_WithSuccessfulResult_ThrowsInvalidOperationExceptionWithMessage()
    {
        // Arrange
        var result = Result.Success();

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => CustomResults.Problem(result));
        exception.Message.ShouldBe("Cannot create problem result from successful result");
    }

    [Fact]
    public void Problem_WithNullError_ThrowsInvalidOperationExceptionWithMessage()
    {
        // Arrange
        var result = Result.Failure(null!);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => CustomResults.Problem(result));
        exception.Message.ShouldBe("Result error cannot be null");
    }
}
