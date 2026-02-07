using Application.ServiceRecords;
using Application.ServiceRecords.GetById;
using Domain.Enums;

namespace ApplicationTests.ServiceRecords.GetById;

public class GetServiceRecordByIdQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetServiceRecordByIdQueryHandler _handler;

    public GetServiceRecordByIdQueryHandlerTests()
    {
        _handler = new GetServiceRecordByIdQueryHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ServiceRecordNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var nonExistentServiceRecordId = Guid.NewGuid();
        var query = new GetServiceRecordByIdQuery(vehicle.Id, nonExistentServiceRecordId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Oil Change", DateTime.UtcNow.AddDays(-5), 10000, 150.00m);
        
        var query = new GetServiceRecordByIdQuery(vehicle.Id, serviceRecord.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ServiceRecordBelongsToDifferentVehicle_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle1 = await CreateVehicleInDb([EnergyType.Gasoline]);
        var vehicle2 = await CreateVehicleInDb([EnergyType.Electric]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle1.Id, serviceType.Id, "Oil Change", DateTime.UtcNow.AddDays(-5), 10000, 150.00m);
        
        var query = new GetServiceRecordByIdQuery(vehicle2.Id, serviceRecord.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ValidServiceRecordWithoutItems_ReturnsServiceRecordDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceDate = new DateTime(2023, 10, 15, 0, 0, 0, DateTimeKind.Utc);
        var serviceRecord = await CreateServiceRecordInDb(
            vehicle.Id, 
            serviceType.Id, 
            "Regular Oil Change", 
            serviceDate, 
            15000, 
            150.50m,
            "Synthetic oil used");

        var query = new GetServiceRecordByIdQuery(vehicle.Id, serviceRecord.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(serviceRecord.Id);
        result.Value.Title.ShouldBe("Regular Oil Change");
        result.Value.Notes.ShouldBe("Synthetic oil used");
        result.Value.Mileage.ShouldBe(15000);
        result.Value.ServiceDate.ShouldBe(serviceDate);
        result.Value.TotalCost.ShouldBe(150.50m);
        result.Value.TypeId.ShouldBe(serviceType.Id);
        result.Value.Type.ShouldBe("Oil Change");
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.ServiceItems.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidServiceRecordWithItems_ReturnsServiceRecordDtoWithItems()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Complete Service");
        var serviceRecord = await CreateServiceRecordInDb(
            vehicle.Id, 
            serviceType.Id, 
            "Full Service Package", 
            DateTime.UtcNow.AddDays(-10), 
            20000, 
            null, // ManualCost should be ignored when items exist
            "Complete service with parts and labor");

        // Add service items
        var item1 = await CreateServiceItemInDb(serviceRecord.Id, "Engine Oil 5W-30", Domain.Enums.Services.ServiceItemType.Part, 45.00m, 5, "OIL-123", "Premium synthetic");
        var item2 = await CreateServiceItemInDb(serviceRecord.Id, "Oil Filter", Domain.Enums.Services.ServiceItemType.Part, 15.00m, 1, "FILTER-456");
        var item3 = await CreateServiceItemInDb(serviceRecord.Id, "Labor", Domain.Enums.Services.ServiceItemType.Labor, 80.00m, 1, null, "Oil change service");

        var query = new GetServiceRecordByIdQuery(vehicle.Id, serviceRecord.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(serviceRecord.Id);
        result.Value.Title.ShouldBe("Full Service Package");
        result.Value.ServiceItems.Count.ShouldBe(3);
        result.Value.TotalCost.ShouldBe(320.00m); // 45*5 + 15*1 + 80*1

        var oilItem = result.Value.ServiceItems.FirstOrDefault(si => si.Name == "Engine Oil 5W-30");
        oilItem.ShouldNotBeNull();
        oilItem.Id.ShouldBe(item1.Id);
        oilItem.Type.ShouldBe(Domain.Enums.Services.ServiceItemType.Part);
        oilItem.UnitPrice.ShouldBe(45.00m);
        oilItem.Quantity.ShouldBe(5);
        oilItem.TotalPrice.ShouldBe(225.00m);
        oilItem.PartNumber.ShouldBe("OIL-123");
        oilItem.Notes.ShouldBe("Premium synthetic");

        var filterItem = result.Value.ServiceItems.FirstOrDefault(si => si.Name == "Oil Filter");
        filterItem.ShouldNotBeNull();
        filterItem.Id.ShouldBe(item2.Id);
        filterItem.TotalPrice.ShouldBe(15.00m);
        filterItem.PartNumber.ShouldBe("FILTER-456");

        var laborItem = result.Value.ServiceItems.FirstOrDefault(si => si.Name == "Labor");
        laborItem.ShouldNotBeNull();
        laborItem.Id.ShouldBe(item3.Id);
        laborItem.Type.ShouldBe(Domain.Enums.Services.ServiceItemType.Labor);
        laborItem.TotalPrice.ShouldBe(80.00m);
    }

    [Fact]
    public async Task Handle_ServiceRecordWithMinimalData_ReturnsServiceRecordDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Basic Service");
        var serviceRecord = await CreateServiceRecordInDb(
            vehicle.Id, 
            serviceType.Id, 
            "Minimal Service", 
            DateTime.UtcNow.AddDays(-1));

        var query = new GetServiceRecordByIdQuery(vehicle.Id, serviceRecord.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(serviceRecord.Id);
        result.Value.Title.ShouldBe("Minimal Service");
        result.Value.Notes.ShouldBeNull();
        result.Value.Mileage.ShouldBeNull();
        result.Value.TotalCost.ShouldBe(0m);
        result.Value.ServiceItems.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task Handle_ValidServiceRecord_IncludesCorrectTimestamps()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Test Service", DateTime.UtcNow.AddDays(-5), 10000, 100.00m);

        var query = new GetServiceRecordByIdQuery(vehicle.Id, serviceRecord.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CreatedDate.ShouldNotBe(default);
        result.Value.UpdatedDate.ShouldNotBe(default);
        result.Value.CreatedDate.ShouldBeLessThanOrEqualTo(result.Value.UpdatedDate);
    }
}