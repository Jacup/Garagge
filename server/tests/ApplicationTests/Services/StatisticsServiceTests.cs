using Application.Abstractions;
using Application.Services;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.Services;

public class StatisticsServiceTests : InMemoryDbTestBase
{
    private readonly StatisticsService _sut;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public StatisticsServiceTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 29));
        _sut = new StatisticsService(Context, _dateTimeProviderMock.Object);
    }

    #region GetVehicleScope Tests

    [Fact]
    public void GetVehicleScope_UserRole_ReturnsOnlyAuthorizedUserVehicles()
    {
        // Arrange
        SetupAuthorizedUser();
        var authorizedUserVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };
        var otherUserVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "BMW",
            Model = "3 Series",
            EngineType = EngineType.Electric,
            UserId = Guid.NewGuid()
        };

        Context.Vehicles.AddRange(authorizedUserVehicle, otherUserVehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(authorizedUserVehicle.Id);
        result[0].UserId.ShouldBe(AuthorizedUserId);
    }

    [Fact]
    public void GetVehicleScope_UserRoleWithNoVehicles_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            UserId = Guid.NewGuid()
        };

        Context.Vehicles.Add(otherUserVehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_UserRoleMultipleVehicles_ReturnsAllAuthorizedUserVehicles()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicles = new[]
        {
            new Vehicle { Id = Guid.NewGuid(), Brand = "Toyota", Model = "Corolla", EngineType = EngineType.Fuel, UserId = AuthorizedUserId },
            new Vehicle { Id = Guid.NewGuid(), Brand = "Honda", Model = "Civic", EngineType = EngineType.Fuel, UserId = AuthorizedUserId },
            new Vehicle { Id = Guid.NewGuid(), Brand = "Tesla", Model = "Model 3", EngineType = EngineType.Electric, UserId = AuthorizedUserId }
        };

        Context.Vehicles.AddRange(vehicles);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.Count.ShouldBe(3);
        result.Select(v => v.Id).ShouldBe(vehicles.Select(v => v.Id));
    }

    [Fact]
    public void GetVehicleScope_UnknownRole_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "UnknownRole").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_AdminRole_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "Admin").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_UserRole_CanIterateMultipleTimes()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        // Act
        var queryable = _sut.GetVehicleScope(AuthorizedUserId, "User");
        var firstIteration = queryable.ToList();
        var secondIteration = queryable.ToList();

        // Assert
        firstIteration.Count.ShouldBe(1);
        secondIteration.Count.ShouldBe(1);
        firstIteration[0].Id.ShouldBe(secondIteration[0].Id);
    }

    [Fact]
    public void GetVehicleScope_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_NullRole_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, null!).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_DifferentUserId_ReturnsEmpty()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = otherUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    #endregion
}

