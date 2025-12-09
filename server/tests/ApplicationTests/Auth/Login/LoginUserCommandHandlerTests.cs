using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Auth;
using Application.Auth.Login;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Moq;
using TestUtils.Fakes;

namespace ApplicationTests.Auth.Login;

public class LoginUserCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenProvider> _tokenProviderMock = new();
    private readonly LoginUserCommandHandler _sut;

    private readonly DateTime _fixedUtcNow = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const int TokenValidityOnRememberMeDays = 30;
    private const int TokenValidityDefaultDays = 1;

    public LoginUserCommandHandlerTests()
    {
        IDateTimeProvider dateTimeProvider = new TestDateTimeProvider(_fixedUtcNow);

        _sut = new LoginUserCommandHandler(Context, _passwordHasherMock.Object, _tokenProviderMock.Object, dateTimeProvider);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnWrongEmailOrPassword()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "password123", false, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(AuthErrors.WrongEmailOrPassword);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldReturnWrongEmailOrPassword()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "wrong-password";
        const string hashedPassword = "hashed-password";

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(password, hashedPassword))
            .Returns(false);

        var command = new LoginUserCommand(email, password, false, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(AuthErrors.WrongEmailOrPassword);
        result.IsFailure.ShouldBeTrue();
        _passwordHasherMock.Verify(x => x.Verify(password, hashedPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnSuccessWithAccessTokenAndRefreshToken()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "correct-password";
        const string hashedPassword = "hashed-password";
        const string expectedToken = "jwt-token-123";
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshToken
        {
            Token = "example-refresh-token",
            SessionDurationDays = TokenValidityDefaultDays,
            ExpiresAt = _fixedUtcNow.AddDays(TokenValidityDefaultDays),
            UserId = userId
        };

        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(password, hashedPassword))
            .Returns(true);

        _tokenProviderMock
            .Setup(x => x.Create(It.IsAny<User>(), It.IsAny<Guid>()))
            .Returns(expectedToken);

        _tokenProviderMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken.Token);

        var command = new LoginUserCommand(email, password, false, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe(expectedToken);
        result.Value.RefreshToken.ShouldBe(refreshToken.Token);

        _passwordHasherMock.Verify(x => x.Verify(password, hashedPassword), Times.Once);
        _tokenProviderMock.Verify(x => x.Create(It.IsAny<User>(), It.IsAny<Guid>()), Times.Once);
        _tokenProviderMock.Verify(x => x.GenerateRefreshToken(), Times.Once);

        var storedRefreshToken = Context.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken.Token);
        storedRefreshToken.ShouldNotBeNull();
        storedRefreshToken.Token.ShouldBe(refreshToken.Token);
        storedRefreshToken.UserId.ShouldBe(refreshToken.UserId);
        storedRefreshToken.ExpiresAt.ShouldBe(refreshToken.ExpiresAt);
    }

    [Fact]
    public async Task Handle_ValidCredentialsWithRememberMeOption_ShouldReturnSuccessWithLongTokenValidity()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "correct-password";
        const string hashedPassword = "hashed-password";
        const string expectedToken = "jwt-token-123";
        var userId = Guid.NewGuid();

        var refreshToken = new RefreshToken
        {
            Token = "example-refresh-token",
            SessionDurationDays = TokenValidityOnRememberMeDays,
            ExpiresAt = _fixedUtcNow.AddDays(TokenValidityOnRememberMeDays),
            UserId = userId
        };

        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(password, hashedPassword))
            .Returns(true);

        _tokenProviderMock
            .Setup(x => x.Create(It.IsAny<User>(), It.IsAny<Guid>()))
            .Returns(expectedToken);

        _tokenProviderMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken.Token);

        var command = new LoginUserCommand(email, password, true, null, null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var storedRefreshToken = Context.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken.Token);
        storedRefreshToken.ShouldNotBeNull();
        storedRefreshToken.Token.ShouldBe(refreshToken.Token);
        storedRefreshToken.UserId.ShouldBe(refreshToken.UserId);
        storedRefreshToken.ExpiresAt.ShouldBe(refreshToken.ExpiresAt);
    }

    [Fact]
    public async Task Handle_ValidCredentialsWithDeviceMetadata_ShouldReturnSuccessWithDeviceMetadataInToken()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "correct-password";
        const string hashedPassword = "hashed-password";
        const string expectedToken = "jwt-token-123";
        const string ipAddress = "127.0.0.1";
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
        
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(password, hashedPassword))
            .Returns(true);

        _tokenProviderMock
            .Setup(x => x.Create(It.IsAny<User>(), It.IsAny<Guid>()))
            .Returns(expectedToken);

        _tokenProviderMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("example-refresh-token");

        var command = new LoginUserCommand(email, password, false, ipAddress, userAgent);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        result.Value.RefreshToken.ShouldBe("example-refresh-token");
        
        var storedRefreshToken = Context.RefreshTokens.SingleOrDefault(rt => rt.Token == "example-refresh-token");
        storedRefreshToken.ShouldNotBeNull();
        storedRefreshToken.ExpiresAt.ShouldBe(_fixedUtcNow.AddDays(TokenValidityDefaultDays));
        storedRefreshToken.IsRevoked.ShouldBe(false);
        storedRefreshToken.IpAddress.ShouldBe(ipAddress);
        storedRefreshToken.UserAgent.ShouldBe(userAgent);
        storedRefreshToken.UserId.ShouldBe(userId);
        storedRefreshToken.DeviceOs.ShouldBe("Windows 10");
        storedRefreshToken.DeviceBrowser.ShouldBe("Chrome");
        storedRefreshToken.DeviceType.ShouldBe("Other");
    }
}