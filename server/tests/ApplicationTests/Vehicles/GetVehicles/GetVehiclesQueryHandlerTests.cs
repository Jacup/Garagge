using Application.Vehicles.GetVehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace ApplicationTests.Vehicles.GetVehicles;

public class GetVehiclesQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetVehiclesQueryHandler _sut;

    public GetVehiclesQueryHandlerTests()
    {
        _sut = new GetVehiclesQueryHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_NoVehicleExistsForLoggedUser_ShouldReturnEmptyPagedList()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicle1 = new Vehicle
        {
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = Guid.NewGuid()
        };
        var vehicle2 = new Vehicle
        {
            Brand = "BMW",
            Model = "3 Series",
            EngineType = EngineType.Electric,
            ManufacturedYear = 2011,
            UserId = Guid.NewGuid()
        };

        Context.Vehicles.AddRange(vehicle1, vehicle2);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Page.ShouldBe(1);
        result.Value.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task Handle_VehiclesExist_ShouldReturnPagedListWithVehicles()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicle1 = new Vehicle
        {
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };
        var vehicle2 = new Vehicle
        {
            Brand = "BMW",
            Model = "3 Series",
            EngineType = EngineType.Electric,
            ManufacturedYear = 2011,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.AddRange(vehicle1, vehicle2);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Page.ShouldBe(1);
        result.Value.PageSize.ShouldBe(10);
        result.Value.HasNextPage.ShouldBeFalse();
        result.Value.HasPreviousPage.ShouldBeFalse();
        result.Value.Items.ShouldAllBe(v => v.UserId == AuthorizedUserId);
    }

    [Fact]
    public async Task Handle_WithPagination_FirstPage_ShouldReturnCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = CreateTestVehicles(5, AuthorizedUserId);
        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(2, 1, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(5);
        result.Value.Page.ShouldBe(1);
        result.Value.PageSize.ShouldBe(2);
        result.Value.HasNextPage.ShouldBeTrue();
        result.Value.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_WithPagination_SecondPage_ShouldReturnCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = CreateTestVehicles(5, AuthorizedUserId);
        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(2, 2, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(5);
        result.Value.Page.ShouldBe(2);
        result.Value.PageSize.ShouldBe(2);
        result.Value.HasNextPage.ShouldBeTrue();
        result.Value.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithPagination_LastPage_ShouldReturnCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = CreateTestVehicles(5, AuthorizedUserId);
        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(2, 3, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.TotalCount.ShouldBe(5);
        result.Value.Page.ShouldBe(3);
        result.Value.PageSize.ShouldBe(2);
        result.Value.HasNextPage.ShouldBeFalse();
        result.Value.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_MatchingBrand_ShouldReturnFilteredResults()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = new List<Vehicle>
        {
            new()
            {
                Brand = "Audi",
                Model = "A4",
                EngineType = EngineType.Fuel,
                ManufacturedYear = 2010,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "BMW",
                Model = "X5",
                EngineType = EngineType.Electric,
                ManufacturedYear = 2011,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "Audi",
                Model = "Q7",
                EngineType = EngineType.Hybrid,
                ManufacturedYear = 2012,
                UserId = AuthorizedUserId
            }
        };

        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, "Audi");

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items.ShouldAllBe(v => v.Brand == "Audi");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_MatchingModel_ShouldReturnFilteredResults()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = new List<Vehicle>
        {
            new()
            {
                Brand = "Audi",
                Model = "A4",
                EngineType = EngineType.Fuel,
                ManufacturedYear = 2010,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "BMW",
                Model = "X5",
                EngineType = EngineType.Electric,
                ManufacturedYear = 2011,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "Mercedes",
                Model = "A4",
                EngineType = EngineType.Electric,
                ManufacturedYear = 2012,
                UserId = AuthorizedUserId
            }
        };

        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, "A4");

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items.ShouldAllBe(v => v.Model == "A4");
    }

    [Fact]
    public async Task Handle_WithSearchTerm_NoMatches_ShouldReturnEmptyResults()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = CreateTestVehicles(3, AuthorizedUserId);
        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, "NonExistingBrand");

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WithSearchTermAndPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = new List<Vehicle>
        {
            new()
            {
                Brand = "Audi",
                Model = "A1",
                EngineType = EngineType.Fuel,
                ManufacturedYear = 2010,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "Audi",
                Model = "A2",
                EngineType = EngineType.Electric,
                ManufacturedYear = 2011,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "Audi",
                Model = "A3",
                EngineType = EngineType.Hybrid,
                ManufacturedYear = 2012,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "BMW",
                Model = "X1",
                EngineType = EngineType.Electric,
                ManufacturedYear = 2013,
                UserId = AuthorizedUserId
            }
        };

        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(2, 1, "Audi");

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(3);
        result.Value.HasNextPage.ShouldBeTrue();
        result.Value.Items.ShouldAllBe(v => v.Brand == "Audi");
    }

    [Fact]
    public async Task Handle_VehiclesOrderedByCreatedDate_ShouldReturnInCorrectOrder()
    {
        // Arrange
        SetupAuthorizedUser();

        var earlierVehicle = new Vehicle
        {
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2010,
            UserId = AuthorizedUserId
        };
        var laterVehicle = new Vehicle
        {
            Brand = "BMW",
            Model = "X5",
            EngineType = EngineType.Electric,
            ManufacturedYear = 2011,
            UserId = AuthorizedUserId
        };

        // Add in reverse order to test sorting
        Context.Vehicles.Add(laterVehicle);
        await Context.SaveChangesAsync();
        await Task.Delay(10); // Ensure different CreatedDate
        Context.Vehicles.Add(earlierVehicle);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.Items[0].Brand.ShouldBe("BMW"); // Should be first (older CreatedDate)
        result.Value.Items[1].Brand.ShouldBe("Audi"); // Should be second (newer CreatedDate)
    }

    [Fact]
    public async Task Handle_VehiclesWithDifferentEngineTypes_ShouldReturnAllVehicles()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicles = new List<Vehicle>
        {
            new()
            {
                Brand = "Tesla",
                Model = "Model 3",
                EngineType = EngineType.Electric,
                ManufacturedYear = 2020,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "Toyota",
                Model = "Prius",
                EngineType = EngineType.Hybrid,
                ManufacturedYear = 2019,
                UserId = AuthorizedUserId
            },
            new()
            {
                Brand = "Audi",
                Model = "A4",
                EngineType = EngineType.Fuel,
                ManufacturedYear = 2018,
                UserId = AuthorizedUserId
            }
        };

        Context.Vehicles.AddRange(vehicles);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
        result.Value.Items.Select(v => v.EngineType).ShouldContain(EngineType.Electric);
        result.Value.Items.Select(v => v.EngineType).ShouldContain(EngineType.Hybrid);
        result.Value.Items.Select(v => v.EngineType).ShouldContain(EngineType.Fuel);
    }

    [Fact]
    public async Task Handle_VehiclesWithOptionalFields_ShouldReturnCompleteData()
    {
        // Arrange
        SetupAuthorizedUser();

        var vehicle = new Vehicle
        {
            Brand = "BMW",
            Model = "X5",
            EngineType = EngineType.Electric,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            VIN = "1HGBH41JXMN109186",
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();

        var request = new GetVehiclesQuery(10, 1, null);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        var returnedVehicle = result.Value.Items[0];
        returnedVehicle.Brand.ShouldBe("BMW");
        returnedVehicle.Model.ShouldBe("X5");
        returnedVehicle.EngineType.ShouldBe(EngineType.Electric);
        returnedVehicle.ManufacturedYear.ShouldBe(2020);
        returnedVehicle.Type.ShouldBe(VehicleType.Car);
        returnedVehicle.VIN.ShouldBe("1HGBH41JXMN109186");
    }

    private static List<Vehicle> CreateTestVehicles(int count, Guid userId)
    {
        var vehicles = new List<Vehicle>();
        var engineTypes = new[] { EngineType.Fuel, EngineType.Electric, EngineType.Hybrid, EngineType.Electric };

        for (int i = 1; i <= count; i++)
        {
            vehicles.Add(new Vehicle
            {
                Brand = $"Brand{i}",
                Model = $"Model{i}",
                EngineType = engineTypes[i % engineTypes.Length],
                ManufacturedYear = 2010 + i,
                UserId = userId
            });
        }

        return vehicles;
    }
}