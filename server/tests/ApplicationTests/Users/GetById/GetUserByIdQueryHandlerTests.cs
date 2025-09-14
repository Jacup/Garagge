using Application.Users;
using Application.Users.GetById;
using Domain.Entities.Users;

namespace ApplicationTests.Users.GetById;

public class GetUserByIdQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetUserByIdQueryHandler _sut;

    public GetUserByIdQueryHandlerTests()
    {
        _sut = new GetUserByIdQueryHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var query = new GetUserByIdQuery(AuthorizedUserId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.NotFound(AuthorizedUserId));
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_UserNotAuthorized_ShouldReturnUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var differentUserId = Guid.NewGuid();
        var query = new GetUserByIdQuery(differentUserId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Error.ShouldBe(UserErrors.Unauthorized);
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_UserAuthorizedAndExists_ShouldReturnUserDto()
    {
        // Arrange
        SetupAuthorizedUser();

        var user = new User
        {
            Id = AuthorizedUserId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password"
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var query = new GetUserByIdQuery(AuthorizedUserId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(user.Id);
        result.Value.Email.ShouldBe(user.Email);
        result.Value.FirstName.ShouldBe(user.FirstName);
        result.Value.LastName.ShouldBe(user.LastName);
    }
}