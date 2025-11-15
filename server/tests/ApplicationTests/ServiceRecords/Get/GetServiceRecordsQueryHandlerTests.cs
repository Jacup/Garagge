using Application.ServiceRecords;
using Application.ServiceRecords.Get;
using Application.Services;
using Application.Vehicles;
using Domain.Entities.Services;
using Domain.Enums;

namespace ApplicationTests.ServiceRecords.Get;

public class GetServiceRecordsQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetServiceRecordsQueryHandler _handler;

    public GetServiceRecordsQueryHandlerTests()
    {
        var filterService = new ServiceRecordFilterService();
        _handler = new GetServiceRecordsQueryHandler(Context, UserContextMock.Object, filterService);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var query = new GetServiceRecordsQuery(nonExistentVehicleId, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(nonExistentVehicleId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_VehicleHasNoServiceRecords_ReturnsEmptyPagedList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_VehicleHasServiceRecords_ReturnsPagedListWithRecords()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var record1 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Oil Change", new DateTime(2023, 10, 15), 1000, 150.50m);
        var record2 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Tire Rotation", new DateTime(2023, 10, 10), 950, 75.00m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items[0].Id.ShouldBe(record1.Id); // Newer entry first (sorted by date desc)
        result.Value.Items[1].Id.ShouldBe(record2.Id);
    }

    [Fact]
    public async Task Handle_WithPagination_FirstPage_ReturnsCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        for (int i = 0; i < 5; i++)
        {
            await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, $"Service {i}", new DateTime(2023, 10, i + 1), 1000 + i * 100, 100m + i * 10);
        }

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
    public async Task Handle_WithPagination_SecondPage_ReturnsCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        for (int i = 0; i < 5; i++)
        {
            await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, $"Service {i}", new DateTime(2023, 10, i + 1), 1000 + i * 100, 100m + i * 10);
        }

        var query = new GetServiceRecordsQuery(vehicle.Id, 2, 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
    public async Task Handle_WithPagination_LastPage_ReturnsCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        for (int i = 0; i < 5; i++)
        {
            await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, $"Service {i}", new DateTime(2023, 10, i + 1), 1000 + i * 100, 100m + i * 10);
        }

        var query = new GetServiceRecordsQuery(vehicle.Id, 3, 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
    public async Task Handle_WithSearchTerm_ReturnsMatchingRecordsInTitle()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Oil Change", new DateTime(2023, 10, 15), 1000, 150m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Tire Rotation", new DateTime(2023, 10, 14), 950, 75m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Oil Filter Replacement", new DateTime(2023, 10, 13), 900, 50m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, SearchTerm: "Oil");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items.ShouldAllBe(sr => sr.Title.Contains("Oil"));
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ReturnsMatchingRecordsInNotes()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 10, 15), 1000, 150m, "Changed synthetic oil");
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 14), 950, 75m, "Regular maintenance");
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 13), 900, 50m, "Used synthetic products");

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, SearchTerm: "synthetic");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WithServiceTypeFilter_ReturnsMatchingRecords()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType1 = await CreateServiceTypeInDb("Oil Change");
        var serviceType2 = await CreateServiceTypeInDb("Tire Service");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType1.Id, "Oil Change 1", new DateTime(2023, 10, 15), 1000, 150m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType2.Id, "Tire Rotation", new DateTime(2023, 10, 14), 950, 75m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType1.Id, "Oil Change 2", new DateTime(2023, 10, 13), 900, 150m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, ServiceTypeId: serviceType1.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items.ShouldAllBe(sr => sr.TypeId == serviceType1.Id);
    }

    [Fact]
    public async Task Handle_WithDateFromFilter_ReturnsRecordsAfterDate()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 10, 1), 1000, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 15), 1050, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 20), 1100, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, DateFrom: new DateTime(2023, 10, 10));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items.ShouldAllBe(sr => sr.ServiceDate >= new DateTime(2023, 10, 10));
    }

    [Fact]
    public async Task Handle_WithDateToFilter_ReturnsRecordsBeforeDate()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 10, 1), 1000, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 15), 1050, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 20), 1100, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, DateTo: new DateTime(2023, 10, 10));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.TotalCount.ShouldBe(1);
        result.Value.Items.ShouldAllBe(sr => sr.ServiceDate <= new DateTime(2023, 10, 10));
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ReturnsRecordsInRange()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 9, 1), 1000, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 5), 1050, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 15), 1100, 100m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 4", new DateTime(2023, 11, 1), 1150, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, DateFrom: new DateTime(2023, 10, 1), DateTo: new DateTime(2023, 10, 31));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items.ShouldAllBe(sr => sr.ServiceDate >= new DateTime(2023, 10, 1) && sr.ServiceDate <= new DateTime(2023, 10, 31));
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ReturnsRecordsMatchingAllFilters()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType1 = await CreateServiceTypeInDb("Oil Change");
        var serviceType2 = await CreateServiceTypeInDb("Tire Service");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType1.Id, "Oil Change Premium", new DateTime(2023, 10, 5), 1000, 150m, "Premium oil used");
        await CreateServiceRecordInDb(vehicle.Id, serviceType1.Id, "Oil Change Standard", new DateTime(2023, 10, 15), 1050, 100m, "Standard oil");
        await CreateServiceRecordInDb(vehicle.Id, serviceType2.Id, "Tire Rotation Premium", new DateTime(2023, 10, 10), 1100, 75m, "Premium service");
        await CreateServiceRecordInDb(vehicle.Id, serviceType1.Id, "Oil Change Premium", new DateTime(2023, 11, 1), 1150, 150m, "Premium oil");

        var query = new GetServiceRecordsQuery(
            vehicle.Id, 
            1, 
            20, 
            SearchTerm: "Premium",
            ServiceTypeId: serviceType1.Id,
            DateFrom: new DateTime(2023, 10, 1),
            DateTo: new DateTime(2023, 10, 31));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        result.Value.TotalCount.ShouldBe(1);
        result.Value.Items[0].Title.ShouldBe("Oil Change Premium");
        result.Value.Items[0].ServiceDate.ShouldBe(new DateTime(2023, 10, 5));
    }

    [Fact]
    public async Task Handle_DefaultSorting_ReturnsSortedByServiceDateDescending()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        var record1 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 10, 1), 1000, 100m);
        var record2 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 15), 1050, 100m);
        var record3 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 10), 1100, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
        result.Value.Items[0].Id.ShouldBe(record2.Id); // 2023-10-15
        result.Value.Items[1].Id.ShouldBe(record3.Id); // 2023-10-10
        result.Value.Items[2].Id.ShouldBe(record1.Id); // 2023-10-01
    }

    [Fact]
    public async Task Handle_SortByServiceDateAscending_ReturnsSortedCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        var record1 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 10, 15), 1000, 100m);
        var record2 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 10), 1050, 100m);
        var record3 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 1), 1100, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, SortBy: "servicedate", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
        result.Value.Items[0].Id.ShouldBe(record3.Id); // 2023-10-01
        result.Value.Items[1].Id.ShouldBe(record2.Id); // 2023-10-10
        result.Value.Items[2].Id.ShouldBe(record1.Id); // 2023-10-15
    }

    [Fact]
    public async Task Handle_SortByMileageDescending_ReturnsSortedCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2023, 10, 15), 1000, 50m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2023, 10, 10), 1500, 150m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2023, 10, 1), 1200, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, SortBy: "mileage", SortDescending: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
        result.Value.Items[0].Mileage.ShouldBe(1500);
        result.Value.Items[1].Mileage.ShouldBe(1200);
        result.Value.Items[2].Mileage.ShouldBe(1000);
    }

    [Fact]
    public async Task Handle_SortByTitleAscending_ReturnsSortedCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "C Service", new DateTime(2023, 10, 15), 1000, 150m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "A Service", new DateTime(2023, 10, 10), 1050, 50m);
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "B Service", new DateTime(2023, 10, 1), 1100, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20, SortBy: "title", SortDescending: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
        result.Value.Items[0].Title.ShouldBe("A Service");
        result.Value.Items[1].Title.ShouldBe("B Service");
        result.Value.Items[2].Title.ShouldBe("C Service");
    }

    [Fact]
    public async Task Handle_ValidRequest_IncludesAllDtoProperties()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceDate = new DateTime(2023, 10, 15, 10, 30, 0);
        var record = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Premium Oil Change", serviceDate, 1000, 150.75m, "Used synthetic oil");

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(1);
        
        var dto = result.Value.Items[0];
        dto.Id.ShouldBe(record.Id);
        dto.Title.ShouldBe("Premium Oil Change");
        dto.Notes.ShouldBe("Used synthetic oil");
        dto.Mileage.ShouldBe(1000);
        dto.ServiceDate.ShouldBe(serviceDate);
        dto.TotalCost.ShouldBe(150.75m);
        dto.TypeId.ShouldBe(serviceType.Id);
        dto.Type.ShouldBe("Oil Change");
        dto.VehicleId.ShouldBe(vehicle.Id);
        dto.CreatedDate.ShouldNotBe(default);
        dto.UpdatedDate.ShouldNotBe(default);
    }

    [Fact]
    public async Task Handle_ServiceRecordWithNullNotes_ReturnsCorrectDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service", new DateTime(2023, 10, 15), 1000, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items[0].Notes.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceRecordWithNullMileage_ReturnsCorrectDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Service", new DateTime(2023, 10, 15), null, 100m);

        var query = new GetServiceRecordsQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items[0].Mileage.ShouldBeNull();
    }
}