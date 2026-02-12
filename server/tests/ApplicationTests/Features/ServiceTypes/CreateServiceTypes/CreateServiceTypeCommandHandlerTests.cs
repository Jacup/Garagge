using Application.Features.ServiceTypes.Create;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.Features.ServiceTypes.CreateServiceTypes;

public class CreateServiceTypesCommandHandlerTests : InMemoryDbTestBase
{
    private readonly CreateServiceTypeCommandHandler _handler;

    public CreateServiceTypesCommandHandlerTests()
    {
        _handler = new CreateServiceTypeCommandHandler(Context);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesServiceTypeWithNewGuid()
    {
        // Arrange
        var name = "Oil Change";
        var command = new CreateServiceTypeCommand(name);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Context.ServiceTypes.AsNoTracking().FirstOrDefault(s => s.Name == name).ShouldNotBeNull();
    }
}