using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
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
    public async Task Login_ValidRequest_ReturnsOk()
    {
        // Arrange
        await CreateUserAsync();
        var loginRequest = new LoginRequest("test@garagge.app", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result?.AccessToken.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_EmailCaseInsensitive_ReturnsOk()
    {
        await CreateUserAsync();
        var request = new LoginRequest("TEST@GARAGGE.APP", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var str = response.Content.ReadAsStringAsync();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result?.AccessToken.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await CreateUserAsync();
        var request = new LoginRequest("test@garagge.app", "WrongPassword", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.WrongEmailOrPassword);
    }

    [Fact]
    public async Task Login_UserNotExists_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.WrongEmailOrPassword);
    }

    [Fact]
    public async Task Login_MissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.MissingEmail);
    }

    [Fact]
    public async Task Login_InvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("invalid-email-format", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.InvalidEmail);
    }

    [Fact]
    public async Task Login_MissingPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest("test@garagge.app", "", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.MissingPassword);
    }

    [Fact]
    public async Task Login_WrongHttpMethod_ReturnsMethodNotAllowed()
    {
        // Act
        var response = await Client.GetAsync(ApiV1Definition.Auth.Login);

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
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_PasswordWithSpecialCharacters_ReturnsOk()
    {
        // Arrange
        const string specialPassword = "P@ssw0rd!#$%^&*()";
        await CreateUserAsync("test@garagge.app", specialPassword);
        var request = new LoginRequest("test@garagge.app", specialPassword, false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result?.AccessToken.ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_EmailWithPlusSign_ReturnsOk()
    {
        // Arrange
        const string emailWithPlus = "test+label@example.com";
        await CreateUserAsync(emailWithPlus);
        var request = new LoginRequest(emailWithPlus, "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result?.AccessToken.ShouldNotBeNull();
    }

    [Fact]
    public async Task Login_WithWhitespaceInEmail_ReturnsOk()
    {
        // Arrange
        await CreateUserAsync();
        var request = new LoginRequest("  test@garagge.app  ", "Password123", false);
        
        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result?.AccessToken.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_SqlInjectionAttemptInEmail_ReturnsBadRequestOrUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("admin'; DROP TABLE Users; --", "Password123", false);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

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
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

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
            .Select(_ => Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert - all should succeed
        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            result?.AccessToken.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Login_WithRememberMeTrue_CreatesLongLivedRefreshToken()
    {
        // Arrange
        var user = await CreateUserAsync();
        var request = new LoginRequest("test@garagge.app", "Password123", true);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

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
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        token.ShouldNotBeNull();
        (token.ExpiresAt - token.CreatedDate).ShouldBeLessThanOrEqualTo(TimeSpan.FromDays(1));
    }
}