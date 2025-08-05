using Application.Abstractions.Authentication;
using Application.Users;
using Application.Users.Register;
using Domain.Entities.Users;
using Moq;

namespace ApplicationTests.Users.Register;

public class RegisterUserCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly RegisterUserCommandHandler _sut;

    public RegisterUserCommandHandlerTests()
    {
        _sut = new RegisterUserCommandHandler(Context, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        const string email = "existing@example.com";
        
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(existingUser);
        await Context.SaveChangesAsync();

        var command = new RegisterUserCommand(email, "John", "Doe", "password123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUserAndReturnUserId()
    {
        // Arrange
        const string email = "new@example.com";
        const string firstName = "John";
        const string lastName = "Doe";
        const string password = "password123";
        const string hashedPassword = "hashed-password123";

        _passwordHasherMock
            .Setup(x => x.Hash(password))
            .Returns(hashedPassword);

        var command = new RegisterUserCommand(email, firstName, lastName, password);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        var createdUser = Context.Users.Single();
        createdUser.Id.ShouldBe(result.Value);
        createdUser.Email.ShouldBe(email);
        createdUser.FirstName.ShouldBe(firstName);
        createdUser.LastName.ShouldBe(lastName);
        createdUser.PasswordHash.ShouldBe(hashedPassword);

        _passwordHasherMock.Verify(x => x.Hash(password), Times.Once);
    }
}
