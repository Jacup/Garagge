using Application.Users;
using Application.Users.Me.Update;
using Domain.Entities.Users;

namespace ApplicationTests.Users.Me.Update;

public class UpdateMeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateMeCommandHandler _sut;
    
    public UpdateMeCommandHandlerTests()
    {
        _sut = new UpdateMeCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateAndReturnValidDto()
    {
        // Arrange
        SetupAuthorizedUser();
        
        const string newEmail = "newemail@garagge.app";
        const string newFirstName = "John";
        const string newLastName = "Smith";

        var user = new User
        {
            Id = AuthorizedUserId,
            Email = "example@garagge.app",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        
        var command = new UpdateMeCommand(newEmail, newFirstName, newLastName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        result.Value.Id.ShouldBe(AuthorizedUserId);
        result.Value.Email.ShouldBe(newEmail);
        result.Value.FirstName.ShouldBe(newFirstName);
        result.Value.LastName.ShouldBe(newLastName);
        
        var userEntity = Context.Users.Single();
        
        userEntity.Id.ShouldBe(AuthorizedUserId);
        userEntity.Email.ShouldBe(newEmail);
        userEntity.FirstName.ShouldBe(newFirstName);
        userEntity.LastName.ShouldBe(newLastName);
    }
    
    [Fact]
    public async Task Handle_UserDoesNotExists_ShouldReturnNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        
        var command = new UpdateMeCommand("example@garagge.app", "John", "Doe");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(UserErrors.NotFound);
    }
    
    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldReturnEmailNotUniqueError()
    {
        // Arrange
        SetupAuthorizedUser();
        
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

        
        var testedUser = new User
        {
            Id = AuthorizedUserId,
            Email = "old@garagge.app",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(testedUser);
        await Context.SaveChangesAsync();
        
        var command = new UpdateMeCommand(existingEmail, testedUser.FirstName, testedUser.LastName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
        result.IsFailure.ShouldBeTrue();
    }
}