using Application.Users;
using Application.Users.Update;
using Domain.Entities.Users;

namespace ApplicationTests.Users.Update;

public class UpdateUserCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _sut = new UpdateUserCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateAndReturnValidDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string newEmail = "newemail@garagge.app";
        const string newFirstName = "John";
        const string newLastName = "Smith";

        var user = new User
        {
            Id = id,
            Email = "example@garagge.app",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        
        var command = new UpdateUserCommand(id, newEmail, newFirstName, newLastName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.Id.ShouldBe(id);
        result.Value.Email.ShouldBe(newEmail);
        result.Value.FirstName.ShouldBe(newFirstName);
        result.Value.LastName.ShouldBe(newLastName);
        
        var userEntity = Context.Users.Single();
        
        userEntity.Id.ShouldBe(id);
        userEntity.Email.ShouldBe(newEmail);
        userEntity.FirstName.ShouldBe(newFirstName);
        userEntity.LastName.ShouldBe(newLastName);
    }
    
    [Fact]
    public async Task Handle_UserDoesNotExists_ShouldReturnNotFoundError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateUserCommand(id, "example@garagge.app", "John", "Doe");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(UserErrors.NotFound(id));
    }
    
    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        const string existingEmail = "existing@garagge.app";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = existingEmail,
            FirstName = "Existing",
            LastName = "User",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(existingUser);
        await Context.SaveChangesAsync();

        var testedUserId = Guid.NewGuid();
        var testedUser = new User
        {
            Id = testedUserId,
            Email = "old@garagge.app",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(testedUser);
        await Context.SaveChangesAsync();
        
        var command = new UpdateUserCommand(testedUserId, existingEmail, testedUser.FirstName, testedUser.LastName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
        result.IsFailure.ShouldBeTrue();
    }
}