using Application.Abstractions.Authentication;
using Application.Auth;
using Application.Auth.ChangePassword;
using Application.Users;
using Domain.Entities.Users;
using Moq;

namespace ApplicationTests.Auth.ChangePassword;

public class ChangePasswordCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly ChangePasswordCommandHandler _sut;

    public ChangePasswordCommandHandlerTests()
    {
        _sut = new ChangePasswordCommandHandler(Context, UserContextMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();

        var command = new ChangePasswordCommand("currentPassword", "newPassword123", false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WrongCurrentPassword_ShouldReturnWrongPasswordError()
    {
        // Arrange
        SetupAuthorizedUser();

        const string currentPassword = "currentPassword";
        const string hashedPassword = "hashedCurrentPassword";

        var user = new User
        {
            Id = AuthorizedUserId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedPassword
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(currentPassword, hashedPassword))
            .Returns(false);

        var command = new ChangePasswordCommand(currentPassword, "newPassword123", false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.PasswordInvalid);
        _passwordHasherMock.Verify(x => x.Verify(currentPassword, hashedPassword), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdatePasswordAndReturnSuccess()
    {
        // Arrange
        SetupAuthorizedUser();

        const string currentPassword = "currentPassword";
        const string newPassword = "newPassword123";
        const string hashedCurrentPassword = "hashedCurrentPassword";
        const string hashedNewPassword = "hashedNewPassword123";

        var user = new User
        {
            Id = AuthorizedUserId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = hashedCurrentPassword
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(currentPassword, hashedCurrentPassword))
            .Returns(true);

        _passwordHasherMock
            .Setup(x => x.Hash(newPassword))
            .Returns(hashedNewPassword);

        var command = new ChangePasswordCommand(currentPassword, newPassword, false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var updatedUser = Context.Users.Single(u => u.Id == AuthorizedUserId);
        updatedUser.PasswordHash.ShouldBe(hashedNewPassword);

        _passwordHasherMock.Verify(x => x.Verify(currentPassword, hashedCurrentPassword), Times.Once);
        _passwordHasherMock.Verify(x => x.Hash(newPassword), Times.Once);
    }
}