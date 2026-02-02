using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using System.Net;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Users;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Users.Me.Sessions;

[Collection("DeleteSessionsTests")]
public class DeleteSessionsTests : BaseIntegrationTest
{
    public DeleteSessionsTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteSessions_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteAllSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteSession_MultipleSessionsExits_DoesNotDeleteCurrentSession()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
        var currentSession = await DbContext.RefreshTokens.FirstAsync(rt => rt.UserId == user.Id);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "SomeRefreshToken",
            ExpiresAt = new DateTimeOffset(),
            SessionDurationDays = 10,
        };
        DbContext.RefreshTokens.Add(refreshToken);
        await DbContext.SaveChangesAsync();

        DbContext.RefreshTokens.Count().ShouldBe(2);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteAllSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        DbContext.RefreshTokens.Count().ShouldBe(1);
        var remainingSession = await DbContext.RefreshTokens.FirstAsync();
        remainingSession.Id.ShouldBe(currentSession.Id);
    }

    [Fact]
    public async Task DeleteSession_WhenSessionsBelongsToAnotherUser_DoesNotDeleteAnotherUserSessions()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "SomeRefreshToken",
            ExpiresAt = new DateTimeOffset(),
            SessionDurationDays = 10,
        };

        var anotherUser = await CreateUserAsync("another@garagge.app", "password", "Another", "User");
        var anotherUserRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = anotherUser.Id,
            Token = "SomeRefreshToken2",
            ExpiresAt = new DateTimeOffset(),
            SessionDurationDays = 10,
        };

        DbContext.RefreshTokens.Add(refreshToken);
        DbContext.RefreshTokens.Add(anotherUserRefreshToken);
        await DbContext.SaveChangesAsync();

        DbContext.RefreshTokens.Count().ShouldBe(3);
        DbContext.RefreshTokens.Count(rt => rt.UserId == user.Id).ShouldBe(2);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteAllSessions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        DbContext.RefreshTokens.Count().ShouldBe(2);
        var remainingUserSession = await DbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).FirstOrDefaultAsync();
        remainingUserSession.ShouldNotBeNull();

        var anotherUserSession = await DbContext.RefreshTokens.Where(rt => rt.UserId == anotherUser.Id).FirstOrDefaultAsync();
        anotherUserSession.ShouldNotBeNull();
    }
}