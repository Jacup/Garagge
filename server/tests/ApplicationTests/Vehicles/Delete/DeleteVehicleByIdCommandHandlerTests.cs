using Application.Abstractions.Data;
using Application.Vehicles;
using Application.Vehicles.Delete;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.Vehicles.Delete;

public class DeleteVehicleByIdCommandHandlerTests : InMemoryDbTestBase
{
    private readonly DeleteVehicleByIdCommandHandler _sut;

    public DeleteVehicleByIdCommandHandlerTests()
    {
        _sut = new DeleteVehicleByIdCommandHandler(Context, UserContextMock.Object);
    }
    
    [Fact]
    public async Task Handle_VehicleNotFound_ShouldReturnNotFoundError()
    {
        SetupAuthorizedUser();
        var requestedId = Guid.NewGuid();
        var request = new DeleteVehicleByIdCommand(requestedId);
        
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ShouldReturnNotFoundError()
    {
        SetupAuthorizedUser();
        
        var otherUser = Guid.NewGuid();
        var requestedId = Guid.NewGuid();
        
        var vehicle = new Vehicle {
            Id = requestedId,
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = otherUser
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        var request = new DeleteVehicleByIdCommand(vehicle.Id);
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound);
    }

    [Fact]
    public async Task Handle_SuccessfulDelete_ShouldReturnSuccess()
    {
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        Context.Vehicles.Count().ShouldBe(1);
        
        var request = new DeleteVehicleByIdCommand(vehicle.Id);
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsSuccess.ShouldBeTrue();
        Context.Vehicles.Count().ShouldBe(0);
    }
}
