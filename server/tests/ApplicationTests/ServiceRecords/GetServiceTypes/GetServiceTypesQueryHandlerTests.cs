using Application.ServiceRecords.GetTypes;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTests.ServiceRecords.GetServiceTypes;

public class GetServiceTypesQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetServiceTypesQueryHandler _handler;
    private readonly Mock<ILogger<GetServiceTypesQueryHandler>> _loggerMock = new();

    public GetServiceTypesQueryHandlerTests()
    {
        _handler = new GetServiceTypesQueryHandler(Context, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NoServiceTypes_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetServiceTypesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ServiceTypesExist_ReturnsServiceTypesCollection()
    {
        // Arrange
        var serviceType1 = await CreateServiceTypeInDb("Oil Change");
        var serviceType2 = await CreateServiceTypeInDb("Tire Rotation");

        var query = new GetServiceTypesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(st => st.Id == serviceType1.Id && st.Name == serviceType1.Name);
        result.Value.ShouldContain(st => st.Id == serviceType2.Id && st.Name == serviceType2.Name);
    }
}