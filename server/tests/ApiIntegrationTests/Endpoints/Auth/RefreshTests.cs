using ApiIntegrationTests.Contracts;
using System.Net;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Auth;

[Collection("RefreshTests")]
public class RefreshTests : BaseIntegrationTest
{
    private readonly CustomWebApplicationFactory _factory;

    public RefreshTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNoContentAndRotatesTokens()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        var oldRefreshToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        var oldTokenValue = oldRefreshToken.Token;

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify new cookies are set
        response.Headers.ShouldContainCookie("accessToken");
        response.Headers.ShouldContainCookie("refreshToken");

        // Verify old token is revoked and replaced
        await DbContext.Entry(oldRefreshToken).ReloadAsync();
        oldRefreshToken.IsRevoked.ShouldBeTrue();
        oldRefreshToken.ReplacedByToken.ShouldNotBeNullOrEmpty();

        // Verify new token exists
        var newRefreshToken = await DbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.UserId == user.Id && !rt.IsRevoked);
        newRefreshToken.ShouldNotBeNull();
        newRefreshToken.Token.ShouldNotBe(oldTokenValue);
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsUnauthorizedAndRevokesToken()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        var refreshToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.TokenExpired);

        await DbContext.Entry(refreshToken).ReloadAsync();
        refreshToken.IsRevoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Refresh_WithReusedToken_RevokesAllUserTokens()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        var initialToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        var initialTokenValue = initialToken.Token;

        // First refresh (legitimate user) - uses automatic cookies from LoginUser
        var response1 = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);
        response1.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act: Second refresh with the old token (attacker with stolen token)
        using var attackerClient = _factory.CreateClient();
        using var request2 = new HttpRequestMessage(HttpMethod.Post, ApiV1Definitions.Auth.Refresh);
        request2.Headers.Add("Cookie", $"refreshToken={initialTokenValue}");
        var response2 = await attackerClient.SendAsync(request2);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var problemDetails = await response2.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.TokenRevoked);

        var allTokens = await DbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        allTokens.ShouldAllBe(rt => rt.IsRevoked);
    }

    [Fact]
    public async Task Refresh_ForLongSession_MaintainsLongSessionDuration()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123", rememberMe: true);

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var newRefreshToken = await DbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .SingleAsync();

        newRefreshToken.SessionDurationDays.ShouldBe(30);
    }

    [Fact]
    public async Task Refresh_ForShortSession_MaintainsShortSessionDuration()
    {
        // Arrange
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123", rememberMe: false);

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var newRefreshToken = await DbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .SingleAsync();

        newRefreshToken.SessionDurationDays.ShouldBe(1);
    }

    [Fact]
    public async Task Refresh_WithoutRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        await CreateUserAsync();

        // Act
        var response = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}