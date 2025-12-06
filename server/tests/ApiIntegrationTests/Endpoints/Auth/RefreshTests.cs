using System.Net;
using System.Net.Http.Json;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Definitions;
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
    public async Task Refresh_WithValidToken_ReturnsNewTokensAndRevokesOld()
    {
        // Arrange
        await CreateUserAsync();
        (_, string initialRefreshToken) = await LoginUser("test@garagge.app", "Password123");
        var oldRefreshToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == initialRefreshToken);

        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={initialRefreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.ShouldNotBeNull();
        loginResponse.AccessToken.ShouldNotBeNullOrEmpty();

        var newRefreshTokenValue = ParseRefreshTokenFromCookie(response.Headers);
        newRefreshTokenValue.ShouldNotBeNull();
        newRefreshTokenValue.ShouldNotBe(initialRefreshToken);

        await DbContext.Entry(oldRefreshToken).ReloadAsync();
        oldRefreshToken.IsRevoked.ShouldBeTrue();
        oldRefreshToken.ReplacedByToken.ShouldBe(newRefreshTokenValue);

        var newRefreshTokenInDb = await DbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == newRefreshTokenValue);
        newRefreshTokenInDb.ShouldNotBeNull();
        newRefreshTokenInDb.IsRevoked.ShouldBeFalse();
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsUnauthorizedAndRevokesToken()
    {
        // Arrange
        await CreateUserAsync();
        (_, string refreshTokenValue) = await LoginUser("test@garagge.app", "Password123");
        
        var refreshToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == refreshTokenValue);
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await DbContext.SaveChangesAsync();

        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken.Token}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        
        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.TokenExpired);

        var expiredCookieValue = ParseRefreshTokenFromCookie(response.Headers, true);
        expiredCookieValue.ShouldBeEmpty();
        
        await DbContext.Entry(refreshToken).ReloadAsync();
        refreshToken.IsRevoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Refresh_WithReusedToken_RevokesAllUserTokens()
    {
        // Arrange
        await CreateUserAsync();
        (_, string refreshToken1) = await LoginUser("test@garagge.app", "Password123");

        // First refresh (legitimate user)
        using var legitimateClient = _factory.CreateClient();
        using var request1 = new HttpRequestMessage(HttpMethod.Post, ApiV1Definition.Auth.Refresh);
        request1.Headers.Add("Cookie", $"refreshToken={refreshToken1}");
        var response1 = await legitimateClient.SendAsync(request1);
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act: Second refresh with the old token (attacker)
        using var attackerClient = _factory.CreateClient();
        using var request2 = new HttpRequestMessage(HttpMethod.Post, ApiV1Definition.Auth.Refresh);
        request2.Headers.Add("Cookie", $"refreshToken={refreshToken1}");
        var response2 = await attackerClient.SendAsync(request2);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var problemDetails = await response2.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(AuthErrors.TokenRevoked);

        var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
        var allTokens = await DbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();
            
        allTokens.ShouldAllBe(rt => rt.IsRevoked);
    }

    [Fact]
    public async Task Refresh_ForLongSession_MaintainsLongSessionDuration()
    {
        // Arrange
        await CreateUserAsync();
        (_, string refreshToken) = await LoginUser("test@garagge.app", "Password123", rememberMe: true);

        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var newRefreshTokenValue = ParseRefreshTokenFromCookie(response.Headers);
        var newRefreshToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == newRefreshTokenValue);

        newRefreshToken.SessionDurationDays.ShouldBe(30);
    }

    [Fact]
    public async Task Refresh_ForShortSession_MaintainsShortSessionDuration()
    {
        // Arrange
        await CreateUserAsync();
        (_, string refreshToken) = await LoginUser("test@garagge.app", "Password123", rememberMe: false);

        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");

        // Act
        var response = await Client.PostAsync(ApiV1Definition.Auth.Refresh, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var newRefreshTokenValue = ParseRefreshTokenFromCookie(response.Headers);
        var newRefreshToken = await DbContext.RefreshTokens.SingleAsync(rt => rt.Token == newRefreshTokenValue);

        newRefreshToken.SessionDurationDays.ShouldBe(1);
    }
}
