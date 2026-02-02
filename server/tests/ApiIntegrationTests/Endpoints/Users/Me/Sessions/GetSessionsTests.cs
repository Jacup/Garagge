using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using System.Net;
using System.Net.Http.Json;
using ApiIntegrationTests.Fixtures;
using Application.Users.Sessions;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Users.Me.Sessions;

[Collection("GetSessionsTests")]
public class GetSessionsTests : BaseIntegrationTest
{
    public GetSessionsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetSessions_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMeSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSessions_WhenUserHasOnlyOneSession_ReturnsOkWithOneSession()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMeSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionsDto>();
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        result.Items.Single().IsCurrent.ShouldBeTrue();
    }

    [Fact]
    public async Task GetSessions_WhenUserHasMultipleSessions_ReturnsOkWithAllSessions()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
        var currentSession = await DbContext.RefreshTokens.FirstAsync(rt => rt.UserId == user.Id);
        
        var otherSession = new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = "other-token", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
        await DbContext.RefreshTokens.AddAsync(otherSession);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMeSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionsDto>();
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(2);
        
        var currentSessionDto = result.Items.Single(s => s.Id == currentSession.Id);
        currentSessionDto.IsCurrent.ShouldBeTrue();
        
        var otherSessionDto = result.Items.Single(s => s.Id == otherSession.Id);
        otherSessionDto.IsCurrent.ShouldBeFalse();
    }
    
    [Fact]
    public async Task GetSessions_WhenUserHasRevokedSessions_DoesNotReturnThem()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
        var currentSession = await DbContext.RefreshTokens.FirstAsync(rt => rt.UserId == user.Id);

        var revokedSession = new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = "revoked-token", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1, IsRevoked = true };
        await DbContext.RefreshTokens.AddAsync(revokedSession);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMeSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SessionsDto>();
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        result.Items.Single().Id.ShouldBe(currentSession.Id);
    }
}
