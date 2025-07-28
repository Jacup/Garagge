using ApplicationTests.Helpers;
using Infrastructure.DAL;
using MediatR;
using Moq;

namespace ApplicationTests;

public abstract class InMemoryDbTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IPublisher> PublisherMock;

    protected InMemoryDbTestBase()
    {
        PublisherMock = new Mock<IPublisher>();
        Context = TestDbContextFactory.Create(PublisherMock);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(Context);
    }
}