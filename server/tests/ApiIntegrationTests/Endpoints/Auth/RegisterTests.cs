using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
using Application.Auth.Register;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Auth;

[Collection("RegisterTests")]
public class RegisterTests : BaseIntegrationTest
{
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Act
        var request = new RegisterUserCommand("test@garagge.app", "John", "Doe", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);
        
        var user = await DbContext.Users.FindAsync(userId);
        
        user.ShouldNotBeNull();
        user.Id.ShouldBe(userId);
        user.FirstName.ShouldBe(request.FirstName);
        user.LastName.ShouldBe(request.LastName);
        user.Email.ShouldBe(request.Email);
        user.PasswordHash.ShouldNotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task Register_MissingEmail_ReturnsBadRequest()
    {
        // Act
        var request = new RegisterUserCommand("", "John", "Doe", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.MissingEmail);
        problemDetails.Errors.ShouldContain(AuthErrors.InvalidEmail);
        
        DbContext.Users.Count().ShouldBe(0);
    }    
    
    [Theory]
    [InlineData("this-is-not-an-email")]
    [InlineData("@garagge.app")]
    [InlineData("test@")]
    public async Task Register_InvalidEmail_ReturnsBadRequest(string email)
    {
        // Act
        var request = new RegisterUserCommand(email, "John", "Doe", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.InvalidEmail);
        
        DbContext.Users.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Register_MissingFirstName_ReturnsBadRequest()
    {
        // Act
        var request = new RegisterUserCommand("test@garagge.app", "", "Doe", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.MissingFirstName);
        
        DbContext.Users.Count().ShouldBe(0);
    }
    
    [Fact]
    public async Task Register_MissingLastName_ReturnsBadRequest()
    {
        // Act
        var request = new RegisterUserCommand("test@garagge.app", "John", "", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.MissingLastName);
        
        DbContext.Users.Count().ShouldBe(0);
    }
    
    [Fact]
    public async Task Register_MissingPassword_ReturnsBadRequest()
    {
        // Act
        var request = new RegisterUserCommand("test@garagge.app", "John", "Doe", "");
        const int passwordLength = 8;
        
        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.InvalidPassword(passwordLength));
        
        DbContext.Users.Count().ShouldBe(0);
    }
    
    [Fact]
    public async Task Register_TooShortPassword_ReturnsBadRequest()
    {
        // Act
        var request = new RegisterUserCommand("test@garagge.app", "John", "Doe", "1234567");
        const int passwordLength = 8;
        
        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.InvalidPassword(passwordLength));
        
        DbContext.Users.Count().ShouldBe(0);
    }
    
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterUserCommand("test-conflict@garagge.app", "John", "Doe", "Password123");
        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);
        
        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        
        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.Title.ShouldBe(AuthErrors.EmailNotUnique.Code);
    }

    [Fact]
    public async Task Register_DuplicateEmailCaseInsensitive_ReturnsConflict()
    {
        // Arrange
        var initialRequest = new RegisterUserCommand("test-case@garagge.app", "John", "Doe", "Password123");
        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, initialRequest);
        
        var duplicateRequest = new RegisterUserCommand("TEST-CASE@GARAGGE.APP", "Jane", "Smith", "Password456");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, duplicateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        
        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.Title.ShouldBe(AuthErrors.EmailNotUnique.Code);
    }

    [Theory]
    [InlineData("  test-whitespace@garagge.app", "John", "Doe")]
    [InlineData("test-whitespace@garagge.app  ", "John", "Doe")]
    public async Task Register_WithWhitespace_ReturnsOkAndTrimsData(string email, string firstName, string lastName)
    {
        // Arrange
        var request = new RegisterUserCommand(email, firstName, lastName, "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        var user = await DbContext.Users.FindAsync(userId);

        user.ShouldNotBeNull();
        user.Email.ShouldBe(email.Trim());
        user.FirstName.ShouldBe(firstName.Trim());
        user.LastName.ShouldBe(lastName.Trim());
    }
}