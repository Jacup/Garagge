using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Vehicles;
using Application.Vehicles.DeleteMyVehicleById;
using Domain.Entities.Vehicles;
using Moq;

namespace ApplicationTests.Vehicles.DeleteMyVehicleById;

public class DeleteMyVehicleByIdCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Guid _loggedUser = Guid.NewGuid();
    
    private readonly DeleteMyVehicleByIdCommandHandler _sut;

    public DeleteMyVehicleByIdCommandHandlerTests()
    {
        _sut = new DeleteMyVehicleByIdCommandHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotAuthorized_ShouldReturnUnauthorizedError()
    {
        _userContextMock
            .Setup(o => o.UserId)
            .Returns(Guid.Empty);
        
        var request = new DeleteMyVehicleByIdCommand(It.IsAny<Guid>());
        
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ShouldReturnNotFoundError()
    {
        SetupAuthorizedUser();
        var requestedId = Guid.NewGuid();
        var request = new DeleteMyVehicleByIdCommand(requestedId);
        
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(requestedId));
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
            ManufacturedYear = 2010,
            UserId = otherUser
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        
        var request = new DeleteMyVehicleByIdCommand(vehicle.Id);
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(requestedId));
    }

    [Fact]
    public async Task Handle_SuccessfulDelete_ShouldReturnSuccess()
    {
        SetupAuthorizedUser();
        
        var vehicle = new Vehicle {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = 2010,
            UserId = _loggedUser
        };
        
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        Context.Vehicles.Count().ShouldBe(1);
        
        var request = new DeleteMyVehicleByIdCommand(vehicle.Id);
        var result = await _sut.Handle(request, CancellationToken.None);
        
        result.IsSuccess.ShouldBeTrue();
        Context.Vehicles.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ExceptionOnDb_ShouldReturnFailure()
    {
        SetupAuthorizedUser();
        var vehicle = new Vehicle {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            ManufacturedYear = 2010,
            UserId = _loggedUser
        };
        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var applicationDbContextMock = new Mock<IApplicationDbContext>();
        applicationDbContextMock.Setup(o => o.Vehicles).Returns(Context.Vehicles);
        applicationDbContextMock.Setup(o => o.SaveChangesAsync(It.IsAny<CancellationToken>())).Throws(new Exception("Database error"));

        var mockedSut = new DeleteMyVehicleByIdCommandHandler(applicationDbContextMock.Object, _userContextMock.Object);
        var request = new DeleteMyVehicleByIdCommand(vehicle.Id);
        var result = await mockedSut.Handle(request, CancellationToken.None);
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(VehicleErrors.DeleteFailed(vehicle.Id));
    }

    private void SetupAuthorizedUser()
    {
        _userContextMock.Setup(o => o.UserId).Returns(_loggedUser);
    }
}
