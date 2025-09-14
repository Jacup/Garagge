using Application.Abstractions.Authentication;
using ApplicationTests.Helpers;
using Infrastructure.DAL;
using MediatR;
using Moq;

namespace ApplicationTests;

public abstract class InMemoryDbTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IPublisher> PublisherMock;
    protected readonly Mock<IUserContext> UserContextMock = new();

    protected readonly Guid AuthorizedUserId = Guid.NewGuid();
    
    protected InMemoryDbTestBase()
    {
        PublisherMock = new Mock<IPublisher>();
        Context = TestDbContextFactory.Create(PublisherMock);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(Context);
    }
    
    protected void SetupAuthorizedUser()
    {
        UserContextMock
            .Setup(o => o.UserId)
            .Returns(AuthorizedUserId);
    }
}