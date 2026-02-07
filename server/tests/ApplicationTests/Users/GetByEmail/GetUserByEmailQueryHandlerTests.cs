using Application.Core;
using Application.Users;
using Application.Users.GetByEmail;
using Domain.Entities.Users;

namespace ApplicationTests.Users.GetByEmail;

public class GetUserByEmailQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetUserByEmailQueryHandler _sut;

    public GetUserByEmailQueryHandlerTests()
    {
        _sut = new GetUserByEmailQueryHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnNotFoundError()
    {
        SetupAuthorizedUser();
        var query = new GetUserByEmailQuery("example@garagge.app");

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Error.ShouldBe(UserErrors.NotFound);
        result.IsFailure.ShouldBeTrue();
    }
    
    [Fact]
    public async Task Handle_UserNotAuthorized_ShouldReturnNotAuthorizedError()
    {
        SetupAuthorizedUser();
        
        const string email = "example@garagge.app";
        Guid userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password123"
        };
        
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var query = new GetUserByEmailQuery("example@garagge.app");

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Error.ShouldBe(UserErrors.NotFound);
        result.IsFailure.ShouldBeTrue();
    }
    
    [Fact]
    public async Task Handle_UserAuthorizedAndExists_ShouldReturnSuccessAndDtoShouldMatchToEntity()
    {
        SetupAuthorizedUser();
        
        const string email = "example@garagge.app";

        var user = new User
        {
            Id = AuthorizedUserId,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed-password123"
        };
        
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var query = new GetUserByEmailQuery("example@garagge.app");

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Error.ShouldBe(Error.None);
        result.IsSuccess.ShouldBeTrue();

        var dto = result.Value;
        
        dto.FirstName.ShouldBe(user.FirstName);
        dto.LastName.ShouldBe(user.LastName);
        dto.Email.ShouldBe(user.Email);
        dto.Id.ShouldBe(user.Id);
    }
}