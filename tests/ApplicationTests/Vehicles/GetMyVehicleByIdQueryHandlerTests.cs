using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.GetMyById;
using Domain.Entities.Vehicles;
using Moq;

namespace ApplicationTests.Vehicles;

public class GetMyVehicleByIdQueryHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IApplicationDbContext> _applicationDbContextMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    private readonly GetMyVehicleByIdQueryHandler _sut;

    private readonly Guid _loggedUser = Guid.NewGuid();
    
    public GetMyVehicleByIdQueryHandlerTests()
    {
        _sut = new GetMyVehicleByIdQueryHandler(_applicationDbContextMock.Object, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserInRequestIsNotTheSameAsLogged_ShouldReturnUnauthorizedError()
    {
        var loggedUserId = Guid.NewGuid();
        var request = new GetMyVehicleByIdQuery(Guid.NewGuid(), It.IsAny<Guid>());
        
        _userContextMock
            .Setup(o => o.UserId)
            .Returns(loggedUserId);

        var result = await _sut.Handle(request, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_VehicleAssignedToOtherUser_ShouldReturnNotFoundError()
    {
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        
        var vehicle = new Vehicle {
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = new DateOnly(2010, 01, 20),
            UserId = otherUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, _userContextMock.Object);
        var request = new GetMyVehicleByIdQuery(_loggedUser, vehicle.Id);
        
        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(vehicle.Id));
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }
    
    [Fact]
    public async Task Handle_VehicleNotExists_ShouldReturnNotFoundError()
    {
        var requestedVehicleId = Guid.NewGuid();
        
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle {
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = new DateOnly(2010, 01, 20),
            UserId = _loggedUser
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, _userContextMock.Object);
        var request = new GetMyVehicleByIdQuery(_loggedUser, requestedVehicleId);
        
        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(requestedVehicleId));
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }
    
    [Fact]
    public async Task Handle_Vehicle_ShouldReturnDto()
    {
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle {
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = new DateOnly(2010, 01, 20),
            UserId = _loggedUser
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, _userContextMock.Object);
        var request = new GetMyVehicleByIdQuery(_loggedUser, vehicle.Id);
        
        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<VehicleDto>();
    }
    
    private void SetupAuthorizedUser()
    {
        _userContextMock
            .Setup(o => o.UserId)
            .Returns(_loggedUser);
    }
}