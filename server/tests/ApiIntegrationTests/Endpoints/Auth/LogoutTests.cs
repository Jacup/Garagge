using ApiIntegrationTests.Contracts;
using System.Net;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Auth;

[Collection("LogoutTests")]
public class LogoutTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Logout_WithValidTokens_ReturnsNoContent()
    {
        // Arrange
        await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithValidTokens_RevokesRefreshToken()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        tokenInDb.IsRevoked.ShouldBeFalse();

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var token = await DbContext.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.UserId == user.Id);
        token.IsRevoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Logout_WithValidTokens_ClearsCookies()
    {
        // Arrange
        await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders).ShouldBeTrue();

        var cookies = setCookieHeaders!.ToList();

        var accessTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("accessToken="));
        accessTokenCookie.ShouldNotBeNull();
        (accessTokenCookie.StartsWith("accessToken=;") || accessTokenCookie.Contains("expires=")).ShouldBeTrue();

        var refreshTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("refreshToken="));
        refreshTokenCookie.ShouldNotBeNull();
        (refreshTokenCookie.StartsWith("refreshToken=;") || refreshTokenCookie.Contains("expires=")).ShouldBeTrue();
    }

    [Fact]
    public async Task Logout_WithoutRefreshToken_ReturnsNoContent()
    {
        // Arrange
        await CreateUserAsync();

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithRevokedRefreshToken_ReturnsNoContent()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        tokenInDb.IsRevoked = true;
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithExpiredRefreshToken_ReturnsNoContent()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        var tokenInDb = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        tokenInDb.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_MultipleLogouts_SecondStillReturnsNoContent()
    {
        // Arrange
        await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        // Act
        var firstLogout = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);
        firstLogout.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var secondLogout = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        secondLogout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_AfterLogout_CannotRefresh()
    {
        // Arrange
        await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        // Act
        var logoutResponse = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var refreshResponse = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);

        // Assert
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var problemDetails = await refreshResponse.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.TokenInvalid);
    }

    [Fact]
    public async Task Logout_WithRememberedSession_RevokesToken()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123", rememberMe: true);

        var tokenInDb = await DbContext.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.UserId == user.Id);
        tokenInDb.SessionDurationDays.ShouldBe(30);

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var token = await DbContext.RefreshTokens.AsNoTracking().SingleAsync(rt => rt.UserId == user.Id);
        token.IsRevoked.ShouldBeTrue();
    }
}