using Application.Abstractions.Authentication;
using Application.Users.Sessions.Get;
using Domain.Entities.Auth;
using Moq;

namespace ApplicationTests.Users.Sessions.Get;

public class GetSessionsQueryHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly GetSessionsQueryHandler _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _currentSessionId = Guid.NewGuid();

    public GetSessionsQueryHandlerTests()
    {
        _userContextMock.Setup(x => x.UserId).Returns(_userId);
        _userContextMock.Setup(x => x.SessionId).Returns(_currentSessionId);

        _sut = new GetSessionsQueryHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetSessionsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserHasSessions_ReturnsSessionsForCurrentUserAndMarksCurrent()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        
        var sessions = new[]
        {
            new RefreshToken { Id = _currentSessionId, UserId = _userId, Token = "token1", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1, CreatedDate = DateTime.UtcNow.AddHours(-1) },
            new RefreshToken { Id = Guid.NewGuid(), UserId = _userId, Token = "token2", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1, CreatedDate = DateTime.UtcNow.AddHours(-2) },
            new RefreshToken { Id = Guid.NewGuid(), UserId = otherUserId, Token = "token3", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1, CreatedDate = DateTime.UtcNow.AddHours(-3) }
        };

        await Context.RefreshTokens.AddRangeAsync(sessions);
        await Context.SaveChangesAsync();

        var query = new GetSessionsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count().ShouldBe(2);
        
        var currentSession = result.Value.Items.Single(s => s.Id == _currentSessionId);
        currentSession.IsCurrent.ShouldBeTrue();

        var otherSession = result.Value.Items.Single(s => s.Id != _currentSessionId);
        otherSession.IsCurrent.ShouldBeFalse();
    }
    
    [Fact]
    public async Task Handle_WhenUserHasRevokedSessions_DoesNotReturnThem()
    {
        // Arrange
        var sessions = new[]
        {
            new RefreshToken { Id = _currentSessionId, UserId = _userId, Token = "token1", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1, CreatedDate = DateTime.UtcNow.AddHours(-1) },
            new RefreshToken { Id = Guid.NewGuid(), UserId = _userId, Token = "token2", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1, IsRevoked = true, CreatedDate = DateTime.UtcNow.AddHours(-2) }
        };

        await Context.RefreshTokens.AddRangeAsync(sessions);
        await Context.SaveChangesAsync();

        var query = new GetSessionsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count().ShouldBe(1);
        result.Value.Items.Single().Id.ShouldBe(_currentSessionId);
    }
    
    [Fact]
    public async Task Handle_WhenUserHasSessions_ReturnsThemSortedByCreatedDateDescending()
    {
        // Arrange
        var session1 = new RefreshToken { Id = Guid.NewGuid(), UserId = _userId, Token = "token1", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
        var session2 = new RefreshToken { Id = _currentSessionId, UserId = _userId, Token = "token2", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };
        var session3 = new RefreshToken { Id = Guid.NewGuid(), UserId = _userId, Token = "token3", ExpiresAt = DateTimeOffset.UtcNow.AddDays(1), SessionDurationDays = 1 };

        Context.RefreshTokens.Add(session1);
        await Context.SaveChangesAsync();
        
        Context.RefreshTokens.Add(session2);
        await Context.SaveChangesAsync();
        
        Context.RefreshTokens.Add(session3);
        await Context.SaveChangesAsync();

        var query = new GetSessionsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var returnedItems = result.Value.Items.ToList();
        
        returnedItems.Count.ShouldBe(3);
        
        returnedItems[0].Id.ShouldBe(session3.Id);
        returnedItems[1].Id.ShouldBe(session2.Id);
        returnedItems[2].Id.ShouldBe(session1.Id);
    }
}
