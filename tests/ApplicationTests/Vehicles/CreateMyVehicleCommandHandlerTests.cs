using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.CreateMyVehicle;
using Moq;

namespace ApplicationTests.Vehicles;

public class CreateMyVehicleCommandHandlerTests : InMemoryDbTestBase
{
    private readonly Mock<IUserContext> _userContextMock = new();

    private readonly CreateMyVehicleCommandHandler _sut;

    private readonly Guid _loggedUser = Guid.NewGuid();

    public CreateMyVehicleCommandHandlerTests()
    {
        _sut = new CreateMyVehicleCommandHandler(Context, _userContextMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotAuthorized_ShouldReturnUnauthorizedError()
    {
        var request = new CreateMyVehicleCommand(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>());

        _userContextMock
            .Setup(o => o.UserId)
            .Returns(Guid.Empty);

        var result = await _sut.Handle(request, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_SuccessfulCreate_ShouldReturnSuccessWithCreatedDto()
    {
        SetupAuthorizedUser();

        var request = new CreateMyVehicleCommand("Audi", "A4", new DateOnly(2010, 01, 20));

        Context.Vehicles.Count().ShouldBe(0);

        var result = await _sut.Handle(request, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBe(Error.None);
        result.Value.ShouldBeOfType<VehicleDto>();

        Context.Vehicles.Count().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_ExceptionOnDb_ShouldReturnFailure()
    {
        SetupAuthorizedUser();

        var request = new CreateMyVehicleCommand("Audi", "A4", new DateOnly(2010, 01, 20));

        var applicationDbContextMock = new Mock<IApplicationDbContext>();
        applicationDbContextMock
            .Setup(o => o.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Throws(new Exception("Database error"));
        
        var mockedSut = new CreateMyVehicleCommandHandler(applicationDbContextMock.Object, _userContextMock.Object);
        
        var result = await mockedSut.Handle(request, CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(VehicleErrors.CreateFailed);
    }

    private void SetupAuthorizedUser()
    {
        _userContextMock
            .Setup(o => o.UserId)
            .Returns(_loggedUser);
    }
}