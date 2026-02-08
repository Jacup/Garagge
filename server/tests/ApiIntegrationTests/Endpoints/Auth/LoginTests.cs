using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
using Application.Users;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Auth;

[Collection("LoginTests")]
public class LoginTests : BaseIntegrationTest
{
    public LoginTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsNoContentAndSetsCookies()
    {
        // Arrange
        await CreateUserAsync();
        var loginRequest = new LoginRequest("test@garagge.app", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.ShouldContainCookie("accessToken");
        response.Headers.ShouldContainCookie("refreshToken");
    }

    [Fact]
    public async Task Login_EmailCaseInsensitive_ReturnsNoContentAndSetsCookies()
    {
        await CreateUserAsync();
        var request = new LoginRequest("TEST@GARAGGE.APP", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.ShouldContainCookie("accessToken");
        response.Headers.ShouldContainCookie("refreshToken");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await CreateUserAsync();
        var request = new LoginRequest("test@garagge.app", "PasswordInvalid", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.CredentialsInvalid);
    }

    [Fact]
    public async Task Login_UserNotExists_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.CredentialsInvalid);
    }

    [Fact]
    public async Task Login_MissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(UserErrors.EmailRequired);
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("invalid-email-format", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(UserErrors.EmailInvalid);
    }

    [Fact]
    public async Task Login_MissingPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("test@garagge.app", "", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(UserErrors.PasswordRequired);
    }

    [Fact]
    public async Task Login_WrongHttpMethod_ReturnsMethodNotAllowed()
    {
        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Auth.Login);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Login_VeryLongPassword_ReturnsUnauthorized()
    {
        // Arrange
        await CreateUserAsync();
        var longPassword = new string('a', 1000);
        var request = new LoginRequest("test@garagge.app", longPassword, false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_PasswordWithSpecialCharacters_ReturnsNoContentAndSetsCookies()
    {
        // Arrange
        const string specialPassword = "P@ssw0rd!#$%^&*()";
        await CreateUserAsync("test@garagge.app", specialPassword);
        var request = new LoginRequest("test@garagge.app", specialPassword, false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.ShouldContainCookie("accessToken");
    }

    [Fact]
    public async Task Login_EmailWithPlusSign_ReturnsNoContentAndSetsCookies()
    {
        // Arrange
        const string emailWithPlus = "test+label@example.com";
        await CreateUserAsync(emailWithPlus);
        var request = new LoginRequest(emailWithPlus, "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.ShouldContainCookie("accessToken");
    }

    [Fact]
    public async Task Login_WithWhitespaceInEmail_ReturnsNoContentAndSetsCookies()
    {
        // Arrange
        await CreateUserAsync();
        var request = new LoginRequest("  test@garagge.app  ", "Password123", false);
        
        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.Headers.ShouldContainCookie("accessToken");
    }

    [Fact]
    public async Task Login_SqlInjectionAttemptInEmail_ReturnsBadRequestOrUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("admin'; DROP TABLE Users; --", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        // Should be either BadRequest (validation) or Unauthorized (user not found)
        (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized).ShouldBeTrue();

        // Verify table still exists
        DbContext.Users.Count().ShouldBeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData("test@garagge.app", "password")] // lowercase
    [InlineData("test@garagge.app", "PASSWORD123")] // uppercase  
    [InlineData("test@garagge.app", "Password")] // missing numbers
    [InlineData("test@garagge.app", "Pass123")] // too short
    public async Task Login_CommonPasswordVariations_ReturnsUnauthorized(string email, string wrongPassword)
    {
        // Arrange
        await CreateUserAsync(email);
        var request = new LoginRequest(email, wrongPassword, false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_MultipleSimultaneousRequests_AllReturnConsistentResults()
    {
        // Arrange
        await CreateUserAsync("concurrent@example.com");
        var request = new LoginRequest("concurrent@example.com", "Password123", false);

        // Act - 5 simultaneous login requests
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert - all should succeed
        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            response.Headers.ShouldContainCookie("accessToken");
        }
    }

    [Fact]
    public async Task Login_WithRememberMeTrue_CreatesLongLivedRefreshToken()
    {
        // Arrange
        var user = await CreateUserAsync();
        var request = new LoginRequest("test@garagge.app", "Password123", true);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var token = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        token.ShouldNotBeNull();
        (token.ExpiresAt - token.CreatedDate).ShouldBeGreaterThan(TimeSpan.FromDays(29));
    }

    [Fact]
    public async Task Login_WithRememberMeFalse_CreatesShortLivedRefreshToken()
    {
        // Arrange
        var user = await CreateUserAsync();
        var request = new LoginRequest("test@garagge.app", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var token = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        token.ShouldNotBeNull();
        (token.ExpiresAt - token.CreatedDate).ShouldBeLessThanOrEqualTo(TimeSpan.FromDays(1));
    }
}