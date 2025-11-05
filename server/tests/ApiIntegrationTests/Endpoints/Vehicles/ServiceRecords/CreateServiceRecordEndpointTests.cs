using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Domain.Enums.Services;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords;

public class CreateServiceRecordEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    #region Authentication & Authorization Tests

    [Fact]
    public async Task CreateServiceRecord_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var request = CreateValidRequest();

        // Act
        var response = 
            await Client.PostAsJsonAsync(ApiV1Definition.Services.Create.WithId(vehicleId),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateServiceRecord_VehicleDoesNotBelongToUser_ReturnsUnauthorized()
    {
        // Arrange
        User owner = await CreateUserAsync("owner@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(owner);
        ServiceType serviceType = await CreateServiceTypeAsync();

        await CreateAndAuthenticateUser();

        var request = CreateValidRequest(serviceType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateServiceRecord_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var nonExistentVehicleId = Guid.NewGuid();
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = CreateValidRequest(serviceType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(nonExistentVehicleId),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateServiceRecord_ServiceTypeDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        var nonExistentServiceTypeId = Guid.NewGuid();
        var request = CreateValidRequest(nonExistentServiceTypeId);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task CreateServiceRecord_ValidData_ReturnsCreatedWithLocation()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");

        var request = CreateValidRequest(serviceType.Id);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateServiceRecord_ValidDataWithAllProperties_ReturnsCorrectDto()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");

        var serviceDate = new DateTime(2024, 11, 5, 10, 30, 0, DateTimeKind.Utc);
        var request = new ServiceRecordCreateRequest(
            Title: "Premium Oil Change",
            Notes: "Used synthetic oil",
            Mileage: 15000,
            ServiceDate: serviceDate,
            ManualCost: 150.75m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe("Premium Oil Change");
        result.Notes.ShouldBe("Used synthetic oil");
        result.Mileage.ShouldBe(15000);
        result.ServiceDate.ShouldBe(serviceDate);
        result.TotalCost.ShouldBe(150.75m);
        result.TypeId.ShouldBe(serviceType.Id);
        result.Type.ShouldBe("Oil Change");
        result.VehicleId.ShouldBe(vehicle.Id);
        result.CreatedDate.ShouldNotBe(default);
        result.UpdatedDate.ShouldNotBe(default);
    }

    [Fact]
    public async Task CreateServiceRecord_ValidData_PersistsToDatabase()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = CreateValidRequest(serviceType.Id, "Test Service");

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        var recordInDb = DbContext.ServiceRecords.FirstOrDefault(sr => sr.Id == result!.Id);
        recordInDb.ShouldNotBeNull();
        recordInDb.Title.ShouldBe("Test Service");
        recordInDb.VehicleId.ShouldBe(vehicle.Id);
        recordInDb.TypeId.ShouldBe(serviceType.Id);
    }

    [Fact]
    public async Task CreateServiceRecord_MinimalData_ReturnsCreatedWithDefaultValues()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Basic Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Basic Service");
        result.Notes.ShouldBeNull();
        result.Mileage.ShouldBeNull();
        result.TotalCost.ShouldBe(0m);
    }

    #endregion

    #region Service Items Tests

    [Fact]
    public async Task CreateServiceRecord_WithMultipleServiceItems_ReturnsCreatedWithAllItems()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");

        var serviceItems = new List<ServiceItemCreateRequest>
        {
            new ServiceItemCreateRequest(
                Name: "Engine Oil 5W-30",
                Type: ServiceItemType.Part,
                UnitPrice: 45.00m,
                Quantity: 5,
                PartNumber: "12345",
                Notes: "Premium synthetic oil"),
            new ServiceItemCreateRequest(
                Name: "Oil Filter",
                Type: ServiceItemType.Part,
                UnitPrice: 15.00m,
                Quantity: 1,
                PartNumber: "67890",
                Notes: null),
            new ServiceItemCreateRequest(
                Name: "Labor",
                Type: ServiceItemType.Labor,
                UnitPrice: 80.00m,
                Quantity: 1,
                PartNumber: null,
                Notes: "Oil change service")
        };

        var request = new ServiceRecordCreateRequest(
            Title: "Complete Oil Change",
            Notes: "Full service with premium oil",
            Mileage: 15000,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: serviceItems);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.ServiceItems.ShouldNotBeNull();
        result.ServiceItems.Count.ShouldBe(3);
        result.TotalCost.ShouldBe(320.00m); // 45*5 + 15*1 + 80*1
    }

    [Fact]
    public async Task CreateServiceRecord_WithServiceItems_CalculatesTotalCostFromItems()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var serviceItems = new List<ServiceItemCreateRequest>
        {
            new ServiceItemCreateRequest("Part A", ServiceItemType.Part, 10.00m, 2, null, null),
            new ServiceItemCreateRequest("Part B", ServiceItemType.Part, 5.00m, 3, null, null)
        };

        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: 100m, // Should be ignored when items exist
            ServiceTypeId: serviceType.Id,
            ServiceItems: serviceItems);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.TotalCost.ShouldBe(35.00m); // 10*2 + 5*3, NOT 100
    }

    [Fact]
    public async Task CreateServiceRecord_WithoutServiceItems_UsesManualCost()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: 125.50m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.TotalCost.ShouldBe(125.50m);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task CreateServiceRecord_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateServiceRecord_TitleExceedsMaxLength_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var longTitle = new string('A', 65); // Max is 64
        var request = new ServiceRecordCreateRequest(
            Title: longTitle,
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateServiceRecord_NotesExceedMaxLength_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var longNotes = new string('A', 501); // Max is 500
        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: longNotes,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateServiceRecord_NegativeMileage_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: null,
            Mileage: -100,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateServiceRecord_NegativeManualCost_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: -50.00m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateServiceRecord_FutureServiceDate_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var futureDate = DateTime.UtcNow.AddDays(1);
        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: futureDate,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task CreateServiceRecord_MaxLengthTitle_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var maxLengthTitle = new string('A', 64);
        var request = new ServiceRecordCreateRequest(
            Title: maxLengthTitle,
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateServiceRecord_MaxLengthNotes_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var maxLengthNotes = new string('A', 500);
        var request = new ServiceRecordCreateRequest(
            Title: "Service",
            Notes: maxLengthNotes,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateServiceRecord_ZeroMileage_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Initial Service",
            Notes: null,
            Mileage: 0,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
        result!.Mileage.ShouldBe(0);
    }

    [Fact]
    public async Task CreateServiceRecord_ZeroCost_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Warranty Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: 0m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
        result!.TotalCost.ShouldBe(0m);
    }

    [Fact]
    public async Task CreateServiceRecord_TodayServiceDate_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var today = DateTime.UtcNow.Date;
        var request = new ServiceRecordCreateRequest(
            Title: "Today Service",
            Notes: null,
            Mileage: null,
            ServiceDate: today,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateServiceRecord_PastServiceDate_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var pastDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var request = new ServiceRecordCreateRequest(
            Title: "Historical Service",
            Notes: null,
            Mileage: null,
            ServiceDate: pastDate,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateServiceRecord_VeryHighMileage_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "High Mileage Service",
            Notes: null,
            Mileage: 999999,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateServiceRecord_VeryHighCost_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var request = new ServiceRecordCreateRequest(
            Title: "Expensive Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: 99999.99m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateServiceRecord_LargeNumberOfServiceItems_ReturnsCreated()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var serviceItems = Enumerable.Range(1, 50)
            .Select(i => new ServiceItemCreateRequest(
                Name: $"Part {i}",
                Type: ServiceItemType.Part,
                UnitPrice: 10.00m,
                Quantity: 1,
                PartNumber: $"P{i:D5}",
                Notes: null))
            .ToList();

        var request = new ServiceRecordCreateRequest(
            Title: "Complex Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id,
            ServiceItems: serviceItems);

        // Act
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.Services.Create.WithId(vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
        result!.ServiceItems.Count.ShouldBe(50);
        result.TotalCost.ShouldBe(500.00m); // 50 * 10
    }

    #endregion

    #region Helper Methods

    private static ServiceRecordCreateRequest CreateValidRequest(
        Guid? serviceTypeId = null,
        string title = "Oil Change")
    {
        return new ServiceRecordCreateRequest(
            Title: title,
            Notes: "Regular maintenance",
            Mileage: 15000,
            ServiceDate: DateTime.UtcNow.AddDays(-1),
            ManualCost: 150.00m,
            ServiceTypeId: serviceTypeId ?? Guid.NewGuid(),
            ServiceItems: new List<ServiceItemCreateRequest>());
    }

    #endregion
}

