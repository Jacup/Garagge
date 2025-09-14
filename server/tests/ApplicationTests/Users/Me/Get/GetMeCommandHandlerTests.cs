using Application.Users;
using Application.Users.Me.Get;
using Domain.Entities.Users;

namespace ApplicationTests.Users.Me.Get;

public class GetMeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly GetMeQueryHandler _sut;

    public GetMeCommandHandlerTests()
    {
        _sut = new GetMeQueryHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task UserExists_ShouldReturnUserDto()
    {
        // Arrange
        var user = new User
        {
            Id = AuthorizedUserId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };
        await Context.Users.AddAsync(user);
        await Context.SaveChangesAsync();
        SetupAuthorizedUser();

        // Act
        var result = await _sut.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe(user.Email);
        result.Value.FirstName.ShouldBe(user.FirstName);
        result.Value.LastName.ShouldBe(user.LastName);
    }

    [Fact]
    public async Task UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthorizedUser();

        // Act
        var result = await _sut.Handle(new GetMeQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.NotFound(AuthorizedUserId));
    }
}