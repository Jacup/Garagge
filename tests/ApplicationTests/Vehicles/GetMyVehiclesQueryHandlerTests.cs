using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.GetMyVehicleById;
using Application.Vehicles.GetMyVehicles;
using Domain.Entities.Vehicles;
using Moq;

namespace ApplicationTests.Vehicles;

public class GetMyVehiclesQueryHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IApplicationDbContext> _applicationDbContextMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    private readonly GetMyVehiclesQueryHandler _mockedSut;
    private readonly GetMyVehiclesQueryHandler _sut;

    private readonly Guid _loggedUser = Guid.NewGuid();
    
    public GetMyVehiclesQueryHandlerTests()
    {
        _mockedSut = new GetMyVehiclesQueryHandler(_applicationDbContextMock.Object, _userContextMock.Object);
        _sut = new GetMyVehiclesQueryHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserInRequestIsNotTheSameAsLogged_ShouldReturnUnauthorizedError()
    {
        var loggedUserId = Guid.NewGuid();
        var request = new GetMyVehiclesQuery(Guid.NewGuid());
        
        _userContextMock
            .Setup(o => o.UserId)
            .Returns(loggedUserId);

        var result = await _mockedSut.Handle(request, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Type.ShouldBe(ErrorType.Unauthorized);
    }
    
    [Fact]
    public async Task Handle_NoVehicleExistsForLoggedUser_ShouldReturnNotFoundError()
    {
        SetupAuthorizedUser();
        
        var vehicle1 = new Vehicle {
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = new DateOnly(2010, 01, 20),
            UserId = Guid.NewGuid()
        };
        var vehicle2 = new Vehicle {
            Brand = "BMW",
            Model = "3 Series",
            ManufacturedYear = new DateOnly(2011, 01, 20),
            UserId = Guid.NewGuid()
        };
        
        Context.Vehicles.AddRange(vehicle1, vehicle2);
        await Context.SaveChangesAsync();

        var request = new GetMyVehiclesQuery(_loggedUser);
        
        var result = await _sut.Handle(request, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFoundForUser(_loggedUser));
        result.Error.Type.ShouldBe(ErrorType.NotFound);
    }
    
    [Fact]
    public async Task Handle_VehicleExists_ShouldReturnDto()
    {
        SetupAuthorizedUser();
        
        var vehicle1 = new Vehicle {
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = new DateOnly(2010, 01, 20),
            UserId = _loggedUser
        };
        var vehicle2 = new Vehicle {
            Brand = "BMW",
            Model = "3 Series",
            ManufacturedYear = new DateOnly(2011, 01, 20),
            UserId = _loggedUser
        };
        
        Context.Vehicles.AddRange(vehicle1, vehicle2);
        await Context.SaveChangesAsync();

        var request = new GetMyVehiclesQuery(_loggedUser);
        
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldBeAssignableTo<ICollection<VehicleDto>>();
    }
    
    private void SetupAuthorizedUser()
    {
        _userContextMock
            .Setup(o => o.UserId)
            .Returns(_loggedUser);
    }
}