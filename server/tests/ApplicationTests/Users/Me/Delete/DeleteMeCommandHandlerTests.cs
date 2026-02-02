using Application.Users;
using Application.Users.Me.Delete;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.Users.Me.Delete;

public class DeleteMeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly DeleteMeCommandHandler _sut;
    public DeleteMeCommandHandlerTests()
    {
        _sut = new DeleteMeCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task UserExists_ShouldDeleteUserAndReturnSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = AuthorizedUserId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hash"
        };
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        SetupAuthorizedUser();

        // Act
        var result = await _sut.Handle(new DeleteMeCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await Context.Users.AnyAsync(u => u.Id == AuthorizedUserId)).ShouldBeFalse();
    }

    [Fact]
    public async Task UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthorizedUser();

        // Act
        var result = await _sut.Handle(new DeleteMeCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.NotFound);
    }
}