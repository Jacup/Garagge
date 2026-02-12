using Application.Features.ServiceTypes.Update;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.Features.ServiceTypes.UpdateServiceTypes;

public class UpdateServiceTypeCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateServiceTypeCommandHandler _handler;

    public UpdateServiceTypeCommandHandlerTests()
    {
        _handler = new UpdateServiceTypeCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesEntityName()
    {
        // Arrange
        var serviceType = await CreateServiceTypeInDb();
        var newName = "New Type";

        var command = new UpdateServiceTypeCommand(serviceType.Id, newName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var resultServiceType = await Context.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(s => s.Id == serviceType.Id);
        resultServiceType.ShouldNotBeNull();
        resultServiceType.Name.ShouldBe(newName);
    }
}