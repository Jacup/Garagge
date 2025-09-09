using Application.Abstractions.Authentication;
using Application.Auth.Login;
using Application.Users;
using Domain.Entities.Users;
using Moq;

namespace ApplicationTests.Users.Login;

public class LoginUserCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenProvider> _tokenProviderMock = new();
    private readonly LoginUserCommandHandler _sut;

    public LoginUserCommandHandlerTests()
    {
        _sut = new LoginUserCommandHandler(Context, _passwordHasherMock.Object, _tokenProviderMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFoundByEmailError()
    {
        // Arrange
        var command = new LoginUserCommand("nonexistent@example.com", "password123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.NotFoundByEmail);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldReturnWrongPasswordError()
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

        var command = new LoginUserCommand(email, password);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.WrongPassword);
        result.IsFailure.ShouldBeTrue();
        _passwordHasherMock.Verify(x => x.Verify(password, hashedPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnSuccessWithAccessToken()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "correct-password";
        const string hashedPassword = "hashed-password";
        const string expectedToken = "jwt-token-123";

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
            .Returns(true);

        _tokenProviderMock
            .Setup(x => x.Create(It.IsAny<User>()))
            .Returns(expectedToken);

        var command = new LoginUserCommand(email, password);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe(expectedToken);
        _passwordHasherMock.Verify(x => x.Verify(password, hashedPassword), Times.Once);
        _tokenProviderMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
    }
}
