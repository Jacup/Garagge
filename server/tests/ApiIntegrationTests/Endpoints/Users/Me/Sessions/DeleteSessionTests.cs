// using ApiIntegrationTests.Contracts;
// using ApiIntegrationTests.Contracts.V1;
// using System.Net;
// using ApiIntegrationTests.Extensions;
// using ApiIntegrationTests.Fixtures;
// using Application.Users;
// using Domain.Entities.Auth;
// using Microsoft.EntityFrameworkCore;
//
// namespace ApiIntegrationTests.Endpoints.Users.Me.Sessions;
//
// [Collection("DeleteSessionTests")]
// public class DeleteSessionTests : BaseIntegrationTest
// {
//     public DeleteSessionTests(CustomWebApplicationFactory factory) : base(factory)
//     {
//     }
//
//     [Fact]
//     public async Task DeleteSession_WhenNotAuthenticated_ReturnsUnauthorized()
//     {
//         // Arrange
//         var sessionId = Guid.NewGuid();
//
//         // Act
//         var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMeSession.WithId(sessionId));
//
//         // Assert
//         response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
//     }
//
//     [Fact]
//     public async Task DeleteSession_WhenDeletingCurrentSession_ReturnsBadRequest()
//     {
//         // Arrange
//         await CreateAndAuthenticateUser();
//         var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
//         var currentSession = await DbContext.RefreshTokens.FirstAsync(rt => rt.UserId == user.Id);
//
//         // Act
//         var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMeSession.WithId(currentSession.Id));
//
//         // Assert
//         response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
//         var problemDetails = await response.GetProblemDetailsAsync();
//         problemDetails.ShouldBeErrorOfType(UserErrors.DeleteCurrentSession);
//     }
//
//     [Fact]
//     public async Task DeleteSession_WhenSessionNotFound_ReturnsNotFound()
//     {
//         // Arrange
//         await CreateAndAuthenticateUser();
//         var nonExistentSessionId = Guid.NewGuid();
//
//         // Act
//         var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMeSession.WithId(nonExistentSessionId));
//
//         // Assert
//         response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//         var problemDetails = await response.GetProblemDetailsAsync();
//         problemDetails.ShouldBeErrorOfType(UserErrors.SessionNotFound(nonExistentSessionId));
//     }
//
//     [Fact]
//     public async Task DeleteSession_WhenSessionBelongsToAnotherUser_ReturnsNotFound()
//     {
//         // Arrange
//         var otherUser = await CreateUserAsync("other@garagge.app");
//         var otherUserSession = new RefreshToken { Id = Guid.NewGuid(), UserId = otherUser.Id, Token = "other-user-token", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
//         await DbContext.RefreshTokens.AddAsync(otherUserSession);
//         await DbContext.SaveChangesAsync();
//         
//         await CreateAndAuthenticateUser();
//
//         // Act
//         var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMeSession.WithId(otherUserSession.Id));
//
//         // Assert
//         response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//     }
//
//     [Fact]
//     public async Task DeleteSession_WhenSessionIsValid_ReturnsNoContentAndDeletesSession()
//     {
//         // Arrange
//         await CreateAndAuthenticateUser();
//         var user = await DbContext.Users.FirstAsync(u => u.Email == "test@garagge.app");
//         
//         var sessionToDelete = new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = "token-to-delete", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
//         await DbContext.RefreshTokens.AddAsync(sessionToDelete);
//         await DbContext.SaveChangesAsync();
//
//         // Act
//         var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMeSession.WithId(sessionToDelete.Id));
//
//         // Assert
//         response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
//         
//         var deletedSession = await DbContext.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(rt => rt.Id == sessionToDelete.Id);
//         deletedSession.ShouldBeNull();
//     }
// }
