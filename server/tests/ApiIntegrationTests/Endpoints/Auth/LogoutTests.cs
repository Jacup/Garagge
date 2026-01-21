using System.Net;
using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Auth;

[Collection("LogoutTests")]
public class LogoutTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Logout_WithValidTokens_ReturnsOk()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        // Add access token to headers and refresh token to cookies
        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithValidTokens_DeletesRefreshToken()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == refreshToken);
        tokenInDb.IsRevoked.ShouldBeFalse();

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify refresh token is revoked
        var tokenExists = await DbContext.RefreshTokens.AnyAsync(rt => rt.Token == refreshToken);
        tokenExists.ShouldBeFalse();
    }

    [Fact]
    public async Task Logout_WithValidTokens_ClearsRefreshTokenCookie()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Check if refreshToken cookie is cleared
        var setCookieHeaders = response.Headers.GetValues("Set-Cookie");
        var refreshTokenCookie = setCookieHeaders.FirstOrDefault(h => h.StartsWith("refreshToken="));

        refreshTokenCookie.ShouldNotBeNull();
        refreshTokenCookie.ShouldStartWith("refreshToken=;");
    }

    [Fact]
    public async Task Logout_WithoutAccessToken_ReturnsNoContent()
    {
        // Arrange - Do NOT authenticate
        await CreateUserAsync();
        var (_, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        // Add only refresh token
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ReturnNoContent()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, _) = await LoginUser("test@garagge.app", "Password123");

        // Authenticate with an access token only, NO refresh token
        Authenticate(loginResponse.AccessToken);

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithRevokedRefreshToken_ReturnsNoContent()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == refreshToken);
        tokenInDb.IsRevoked = true;
        await DbContext.SaveChangesAsync();

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithExpiredRefreshToken_ReturnsNoContent()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == refreshToken);
        tokenInDb.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await DbContext.SaveChangesAsync();

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithInvalidAccessToken_ReturnsNoContent()
    {
        // Arrange
        await CreateUserAsync();
        var (_, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        // Authenticate with invalid access token
        Authenticate("invalid.jwt.token");
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_MultipleLogouts_SecondStillReturnNoContent()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act - First logout
        var firstLogout = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);
        firstLogout.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        Client.DefaultRequestHeaders.Remove("Cookie");
        // Try second logout with same token
        var secondLogout = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        secondLogout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_AfterLogout_CannotRefresh()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123");

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act - Logout
        var logoutResponse = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Clear authorization header for refresh attempt
        Client.DefaultRequestHeaders.Authorization = null;

        // Try to refresh with the same refresh token
        var refreshResponse = await Client.PostAsync(ApiV1Definition.Auth.Refresh, null);

        // Assert
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var problemDetails = await refreshResponse.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.InvalidToken);
    }

    [Fact]
    public async Task Logout_WithRememberedSession_DeletesToken()
    {
        // Arrange
        await CreateUserAsync();
        var (loginResponse, refreshToken) = await LoginUser("test@garagge.app", "Password123", rememberMe: true);

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == refreshToken);
        tokenInDb.SessionDurationDays.ShouldBe(30);

        Authenticate(loginResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var tokenExists = await DbContext.RefreshTokens.AnyAsync(rt => rt.Token == refreshToken);
        tokenExists.ShouldBeFalse();
    
    }
}