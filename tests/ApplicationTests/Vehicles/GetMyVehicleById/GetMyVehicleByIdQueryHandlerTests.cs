using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.GetMyVehicleById;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.Vehicles.GetMyVehicleById;

public class GetMyVehicleByIdQueryHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IApplicationDbContext> _applicationDbContextMock = new();

    private readonly GetMyVehicleByIdQueryHandler _sut;
    
    public GetMyVehicleByIdQueryHandlerTests()
    {
        _sut = new GetMyVehicleByIdQueryHandler(_applicationDbContextMock.Object, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserInRequestIsNotTheSameAsLogged_ShouldReturnUnauthorizedError()
    {
        var request = new GetMyVehicleByIdQuery(Guid.NewGuid(), It.IsAny<Guid>());
        
        UserContextMock
            .Setup(o => o.UserId)
            .Returns(LoggedUserId);

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
            PowerType = PowerType.Gasoline,
            UserId = otherUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, UserContextMock.Object);
        var request = new GetMyVehicleByIdQuery(LoggedUserId, vehicle.Id);
        
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
            PowerType = PowerType.Gasoline,
            ManufacturedYear = 2010,
            UserId = LoggedUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, UserContextMock.Object);
        var request = new GetMyVehicleByIdQuery(LoggedUserId, requestedVehicleId);
        
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
            PowerType = PowerType.Gasoline,
            UserId = LoggedUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, UserContextMock.Object);
        var request = new GetMyVehicleByIdQuery(LoggedUserId, vehicle.Id);
        
        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<VehicleDto>();
        result.Value.Brand.ShouldBe("Audi");
        result.Value.Model.ShouldBe("A4");
        result.Value.PowerType.ShouldBe(PowerType.Gasoline);
        result.Value.UserId.ShouldBe(LoggedUserId);
    }

    [Fact]
    public async Task Handle_VehicleWithAllOptionalFields_ShouldReturnCompleteDto()
    {
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle {
            Brand = "BMW",
            Model = "X5",
            PowerType = PowerType.Hybrid,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109186",
            UserId = LoggedUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, UserContextMock.Object);
        var request = new GetMyVehicleByIdQuery(LoggedUserId, vehicle.Id);
        
        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("BMW");
        result.Value.Model.ShouldBe("X5");
        result.Value.PowerType.ShouldBe(PowerType.Hybrid);
        result.Value.ManufacturedYear.ShouldBe(2020);
        result.Value.Type.ShouldBe(VehicleType.Car);
        result.Value.VIN.ShouldBe("1HGBH41JXMN109186");
        result.Value.UserId.ShouldBe(LoggedUserId);
    }

    [Fact]
    public async Task Handle_VehicleWithMinimalData_ShouldReturnDtoWithNullOptionalFields()
    {
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle {
            Brand = "Tesla",
            Model = "Model 3",
            PowerType = PowerType.Electric,
            ManufacturedYear = null,
            Type = null,
            VIN = null,
            UserId = LoggedUserId
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var sut = new GetMyVehicleByIdQueryHandler(Context, UserContextMock.Object);
        var request = new GetMyVehicleByIdQuery(LoggedUserId, vehicle.Id);
        
        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Brand.ShouldBe("Tesla");
        result.Value.Model.ShouldBe("Model 3");
        result.Value.PowerType.ShouldBe(PowerType.Electric);
        result.Value.ManufacturedYear.ShouldBeNull();
        result.Value.Type.ShouldBeNull();
        result.Value.VIN.ShouldBeNull();
        result.Value.UserId.ShouldBe(LoggedUserId);
    }
    

}