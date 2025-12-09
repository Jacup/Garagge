using Application.Abstractions.Authentication;
using Application.Users;
using Application.Users.Sessions.Delete;
using Domain.Entities.Auth;
using Moq;

namespace ApplicationTests.Users.Sessions.Delete;

public class DeleteSessionCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly DeleteSessionCommandHandler _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _currentSessionId = Guid.NewGuid();

    public DeleteSessionCommandHandlerTests()
    {
        _userContextMock.Setup(x => x.UserId).Returns(_userId);
        _userContextMock.Setup(x => x.SessionId).Returns(_currentSessionId);

        _sut = new DeleteSessionCommandHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSessionIdIsCurrentSession_ReturnsFailure()
    {
        // Arrange
        var command = new DeleteSessionCommand(_currentSessionId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.DeleteCurrentSessionFailed);
    }

    [Fact]
    public async Task Handle_WhenSessionNotFound_ReturnsFailure()
    {
        // Arrange
        var nonExistentSessionId = Guid.NewGuid();
        var command = new DeleteSessionCommand(nonExistentSessionId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.SessionNotFound(nonExistentSessionId));
    }

    [Fact]
    public async Task Handle_WhenSessionBelongsToAnotherUser_ReturnsFailure()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var otherUserSession = new RefreshToken { Id = Guid.NewGuid(), UserId = otherUserId, Token = "other-user-token", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
        
        await Context.RefreshTokens.AddAsync(otherUserSession);
        await Context.SaveChangesAsync();
        
        var command = new DeleteSessionCommand(otherUserSession.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.SessionNotFound(otherUserSession.Id));
    }

    [Fact]
    public async Task Handle_WhenSessionIsValid_ReturnsSuccessAndDeletesSession()
    {
        // Arrange
        var sessionToDelete = new RefreshToken { Id = Guid.NewGuid(), UserId = _userId, Token = "token-to-delete", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
        
        await Context.RefreshTokens.AddAsync(sessionToDelete);
        await Context.SaveChangesAsync();
        
        var command = new DeleteSessionCommand(sessionToDelete.Id);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var deletedSession = await Context.RefreshTokens.FindAsync(sessionToDelete.Id);
        deletedSession.ShouldBeNull();
    }
}
