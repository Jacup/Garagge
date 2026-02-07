using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Auth;
using Application.Auth.Refresh;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests.Auth.Refresh;

public class RefreshTokenCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<ITokenProvider> _tokenProviderMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock = new();
    private readonly RefreshTokenCommandHandler _sut;

    private readonly DateTime _fixedUtcNow = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "test@test.com", PasswordHash = "password-hash", FirstName = "test", LastName = "test" };
    
    public RefreshTokenCommandHandlerTests()
    {
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_fixedUtcNow);

        _sut = new RefreshTokenCommandHandler(
            Context,
            _tokenProviderMock.Object,
            _dateTimeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ShouldReturnInvalidToken()
    {
        // Arrange
        var command = new RefreshTokenCommand("non-existent-token", "some-ip", "some-user-agent");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(AuthErrors.TokenInvalid);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_TokenIsRevoked_ShouldReturnTokenRevokedAndRevokeAllUserTokens()
    {
        // Arrange
        
        var activeToken = new RefreshToken { Id = Guid.NewGuid(), UserId = _user.Id, Token = "active-token", ExpiresAt = _fixedUtcNow.AddDays(1), SessionDurationDays = 1 };
        var revokedToken = new RefreshToken { Id = Guid.NewGuid(), UserId = _user.Id, Token = "revoked-token", IsRevoked = true, ExpiresAt = _fixedUtcNow.AddDays(1), SessionDurationDays = 1};
        
        Context.Users.Add(_user);
        Context.RefreshTokens.AddRange(activeToken, revokedToken);
        await Context.SaveChangesAsync();

        var command = new RefreshTokenCommand(revokedToken.Token, "some-ip", "some-user-agent");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(AuthErrors.TokenRevoked);
        result.IsFailure.ShouldBeTrue();

        var allTokens = await Context.RefreshTokens.Where(t => t.UserId == _user.Id).ToListAsync();
        allTokens.ShouldAllBe(t => t.IsRevoked);
    }

    [Fact]
    public async Task Handle_TokenIsExpired_ShouldReturnTokenExpired()
    {
        // Arrange
        var expiredToken = new RefreshToken { Id = Guid.NewGuid(), UserId = _user.Id, Token = "expired-token", ExpiresAt = _fixedUtcNow.AddDays(-1), SessionDurationDays = 1 };

        Context.Users.Add(_user);
        Context.RefreshTokens.Add(expiredToken);
        await Context.SaveChangesAsync();

        var command = new RefreshTokenCommand(expiredToken.Token, "some-ip", "some-user-agent");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(AuthErrors.TokenExpired);
        result.IsFailure.ShouldBeTrue();

        var tokenInDb = await Context.RefreshTokens.SingleAsync(t => t.Id == expiredToken.Id);
        tokenInDb.IsRevoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldReturnNewTokensAndRevokeOldOne()
    {
        // Arrange
        var oldToken = new RefreshToken { Id = Guid.NewGuid(), UserId = _user.Id, Token = "valid-token", ExpiresAt = _fixedUtcNow.AddDays(1), SessionDurationDays = 1};
        var otherExpiredToken = new RefreshToken { Id = Guid.NewGuid(), UserId = _user.Id, Token = "other-expired", ExpiresAt = _fixedUtcNow.AddDays(-1), SessionDurationDays = 1};
        
        Context.Users.Add(_user);
        Context.RefreshTokens.AddRange(oldToken, otherExpiredToken);
        await Context.SaveChangesAsync();

        const string newAccessToken = "new-access-token";
        const string newRefreshToken = "new-refresh-token";
        const string ipAddress = "127.0.0.1";
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";

        _tokenProviderMock.Setup(x => x.Create(_user, It.IsAny<Guid>())).Returns(newAccessToken);
        _tokenProviderMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);

        var command = new RefreshTokenCommand(oldToken.Token, ipAddress, userAgent);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe(newAccessToken);
        result.Value.RefreshToken.ShouldBe(newRefreshToken);

        var oldTokenInDb = await Context.RefreshTokens.SingleAsync(t => t.Id == oldToken.Id);
        oldTokenInDb.IsRevoked.ShouldBeTrue();
        oldTokenInDb.ReplacedByToken.ShouldBe(newRefreshToken);

        var newTokenInDb = await Context.RefreshTokens.SingleOrDefaultAsync(t => t.Token == newRefreshToken);
        newTokenInDb.ShouldNotBeNull();
        newTokenInDb.IsRevoked.ShouldBeFalse();
        newTokenInDb.UserId.ShouldBe(_user.Id);
        newTokenInDb.IpAddress.ShouldBe(ipAddress);
        newTokenInDb.UserAgent.ShouldBe(userAgent);
        newTokenInDb.DeviceOs.ShouldBe("Windows 10");
        newTokenInDb.DeviceBrowser.ShouldBe("Chrome");
        newTokenInDb.DeviceType.ShouldBe("Other");
        
        var shouldBeDeletedToken = await Context.RefreshTokens.SingleOrDefaultAsync(t => t.Id == otherExpiredToken.Id);
        shouldBeDeletedToken.ShouldBeNull();
    }

    // In-memory DB failure. 
    // [Fact]
    // public async Task Handle_TokenWithNoUser_ShouldReturnUserNotFound()
    // {
    //     // Arrange
    //     var orphanedToken = new RefreshToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Token = "orphaned-token", ExpiresAt = _fixedUtcNow.AddDays(1), SessionDurationDays = 1 };
    //     
    //     Context.RefreshTokens.Add(orphanedToken);
    //     
    //     await Context.SaveChangesAsync();
    //
    //     var command = new RefreshTokenCommand(orphanedToken.Token, null, null);
    //
    //     // Act
    //     var result = await _sut.Handle(command, CancellationToken.None);
    //
    //     // Assert
    //     result.Error.ShouldBe(AuthErrors.UserNotFound);
    //     result.IsFailure.ShouldBeTrue();
    // }

    [Theory]
    [InlineData(30)]
    [InlineData(1)]
    public async Task Handle_ValidToken_ShouldMaintainSessionDuration(int sessionDuration)
    {
        // Arrange
        var oldToken = new RefreshToken { Id = Guid.NewGuid(), UserId = _user.Id, Token = "valid-token", ExpiresAt = _fixedUtcNow.AddDays(sessionDuration), SessionDurationDays = sessionDuration};
        
        Context.Users.Add(_user);
        Context.RefreshTokens.Add(oldToken);
        await Context.SaveChangesAsync();

        _tokenProviderMock.Setup(x => x.Create(_user, It.IsAny<Guid>())).Returns("new-access-token");
        _tokenProviderMock.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh-token");

        var command = new RefreshTokenCommand(oldToken.Token, "some-ip", "some-user-agent");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var newTokenInDb = await Context.RefreshTokens.SingleAsync(t => t.Token == "new-refresh-token");
        newTokenInDb.SessionDurationDays.ShouldBe(sessionDuration);
        newTokenInDb.ExpiresAt.ShouldBe(_fixedUtcNow.AddDays(sessionDuration));
    }
}