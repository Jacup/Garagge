using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords;

public class UpdateServiceRecordEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    #region Authentication & Authorization Tests

    [Fact]
    public async Task UpdateServiceRecord_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var serviceRecordId = Guid.NewGuid();
        var request = CreateValidUpdateRequest();

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicleId, serviceRecordId),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateServiceRecord_ServiceRecordDoesNotBelongToUser_ReturnsUnauthorized()
    {
        // Arrange
        User owner = await CreateUserAsync("owner@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(owner);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        await CreateAndAuthenticateUser();
        var request = CreateValidUpdateRequest(serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateServiceRecord_ServiceRecordDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        var nonExistentServiceRecordId = Guid.NewGuid();

        var request = CreateValidUpdateRequest(serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, nonExistentServiceRecordId),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateServiceRecord_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        var nonExistentVehicleId = Guid.NewGuid();

        var request = CreateValidUpdateRequest(serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, nonExistentVehicleId, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateServiceRecord_ServiceRecordBelongsToDifferentVehicle_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle1 = await CreateVehicleAsync(user);
        Vehicle vehicle2 = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle1.Id, serviceType.Id);

        var request = CreateValidUpdateRequest(serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle2.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateServiceRecord_ServiceTypeDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var nonExistentServiceTypeId = Guid.NewGuid();
        var request = CreateValidUpdateRequest(nonExistentServiceTypeId);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task UpdateServiceRecord_ValidData_ReturnsOkWithUpdatedDto()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType oldServiceType = await CreateServiceTypeAsync("Oil Change");
        ServiceType newServiceType = await CreateServiceTypeAsync("Inspection");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            oldServiceType.Id,
            "Old Title",
            "Old notes",
            10000,
            100.00m);

        var serviceDate = new DateTime(2024, 11, 1, 10, 0, 0, DateTimeKind.Utc);
        var request = new ServiceRecordUpdateRequest(
            Title: "Updated Oil Change",
            Notes: "Updated notes - synthetic oil used",
            Mileage: 20000,
            ServiceDate: serviceDate,
            ManualCost: 200.00m,
            ServiceTypeId: newServiceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceRecord.Id);
        result.Title.ShouldBe("Updated Oil Change");
        result.Notes.ShouldBe("Updated notes - synthetic oil used");
        result.Mileage.ShouldBe(20000);
        result.ServiceDate.ShouldBe(serviceDate);
        result.TotalCost.ShouldBe(200.00m);
        result.TypeId.ShouldBe(newServiceType.Id);
        result.Type.ShouldBe("Inspection");
        result.VehicleId.ShouldBe(vehicle.Id);
        result.UpdatedDate.ShouldBeGreaterThan(result.CreatedDate);
    }

    [Fact]
    public async Task UpdateServiceRecord_ValidData_PersistsChangesToDatabase()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "Updated Title",
            Notes: "Updated notes",
            Mileage: 25000,
            ServiceDate: new DateTime(2024, 10, 15, 0, 0, 0, DateTimeKind.Utc),
            ManualCost: 300.00m,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Clear change tracker to force reload from database
        DbContext.ChangeTracker.Clear();
        
        var updatedRecord = DbContext.ServiceRecords.First(sr => sr.Id == serviceRecord.Id);
        updatedRecord.Title.ShouldBe("Updated Title");
        updatedRecord.Notes.ShouldBe("Updated notes");
        updatedRecord.Mileage.ShouldBe(25000);
        updatedRecord.ServiceDate.ShouldBe(new DateTime(2024, 10, 15, 0, 0, 0, DateTimeKind.Utc));
        updatedRecord.ManualCost.ShouldBe(300.00m);
        updatedRecord.TypeId.ShouldBe(serviceType.Id);
    }

    [Fact]
    public async Task UpdateServiceRecord_WithNullOptionalValues_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Original Title");

        var request = new ServiceRecordUpdateRequest(
            Title: "Minimal Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Minimal Service");
        result.Notes.ShouldBeNull();
        result.Mileage.ShouldBeNull();
        result.TotalCost.ShouldBe(0m);
    }

    [Fact]
    public async Task UpdateServiceRecord_ChangeServiceType_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType oilChangeType = await CreateServiceTypeAsync("Oil Change");
        ServiceType inspectionType = await CreateServiceTypeAsync("Inspection");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, oilChangeType.Id);

        var request = CreateValidUpdateRequest(inspectionType.Id, "Changed to Inspection");

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.TypeId.ShouldBe(inspectionType.Id);
        result.Type.ShouldBe("Inspection");
    }

    [Fact]
    public async Task UpdateServiceRecord_WithZeroManualCost_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "Free Service",
            Notes: "Warranty repair",
            Mileage: 10000,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: 0m,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.TotalCost.ShouldBe(0m);
    }

    [Fact]
    public async Task UpdateServiceRecord_WithZeroMileage_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "First Service",
            Notes: "Brand new vehicle",
            Mileage: 0,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: 50.00m,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.Mileage.ShouldBe(0);
    }

    #endregion

    #region Service Items Preservation Tests

    [Fact]
    public async Task UpdateServiceRecord_WithExistingServiceItems_DoesNotModifyServiceItems()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        
        // Add service items
        await CreateServiceItemAsync(serviceRecord.Id, "Oil Filter");
        await CreateServiceItemAsync(serviceRecord.Id, "Engine Oil");

        var request = new ServiceRecordUpdateRequest(
            Title: "Updated Service",
            Notes: "Updated notes",
            Mileage: 20000,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.ServiceItems.Count.ShouldBe(2);
        result.ServiceItems.Any(si => si.Name == "Oil Filter").ShouldBeTrue();
        result.ServiceItems.Any(si => si.Name == "Engine Oil").ShouldBeTrue();

        // Verify in database
        DbContext.ChangeTracker.Clear();
        var itemsInDb = DbContext.ServiceItems.Where(si => si.ServiceRecordId == serviceRecord.Id).ToList();
        itemsInDb.Count.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateServiceRecord_WithServiceItems_IgnoresManualCostInTotalCostCalculation()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        
        // Add service items (total: 10*2 + 20*1 = 40)
        await CreateServiceItemAsync(serviceRecord.Id, "Part A", 10.00m, 2);
        await CreateServiceItemAsync(serviceRecord.Id, "Part B", 20.00m);

        var request = new ServiceRecordUpdateRequest(
            Title: "Updated Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: 100.00m, // Should be ignored
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.TotalCost.ShouldBe(40.00m); // From items, not from ManualCost
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task UpdateServiceRecord_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceRecord_TitleExceedsMaxLength_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var longTitle = new string('A', 65); // Max is 64
        var request = new ServiceRecordUpdateRequest(
            Title: longTitle,
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceRecord_NotesExceedMaxLength_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var longNotes = new string('A', 501); // Max is 500
        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: longNotes,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceRecord_NegativeMileage_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: null,
            Mileage: -100,
            ServiceDate: DateTime.UtcNow,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceRecord_NegativeManualCost_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow,
            ManualCost: -50.00m,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceRecord_FutureServiceDate_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var futureDate = DateTime.UtcNow.AddDays(7);
        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: futureDate,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceRecord_TitleAtMaxLength_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var maxLengthTitle = new string('A', 64); // Exactly max length
        var request = new ServiceRecordUpdateRequest(
            Title: maxLengthTitle,
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateServiceRecord_NotesAtMaxLength_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var maxLengthNotes = new string('A', 500); // Exactly max length
        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: maxLengthNotes,
            Mileage: null,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateServiceRecord_ServiceDateToday_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var today = DateTime.UtcNow.Date;
        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: today,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateServiceRecord_ServiceDateInPast_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var pastDate = DateTime.UtcNow.AddMonths(-6);
        var request = new ServiceRecordUpdateRequest(
            Title: "Service",
            Notes: null,
            Mileage: null,
            ServiceDate: pastDate,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task UpdateServiceRecord_UpdateSameDataMultipleTimes_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = CreateValidUpdateRequest(serviceType.Id);

        // Act - Update multiple times
        var response1 = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);
        
        var response2 = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateServiceRecord_UpdateToMinimalValues_ClearsOptionalFields()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Full Service",
            "Detailed notes here",
            50000,
            500.00m);

        var request = new ServiceRecordUpdateRequest(
            Title: "Basic Service",
            Notes: null,
            Mileage: null,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: null,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        DbContext.ChangeTracker.Clear();
        var updatedRecord = DbContext.ServiceRecords.First(sr => sr.Id == serviceRecord.Id);
        updatedRecord.Title.ShouldBe("Basic Service");
        updatedRecord.Notes.ShouldBeNull();
        updatedRecord.Mileage.ShouldBeNull();
        updatedRecord.ManualCost.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateServiceRecord_LargeValues_UpdatesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var request = new ServiceRecordUpdateRequest(
            Title: "Expensive Service",
            Notes: null,
            Mileage: 999999,
            ServiceDate: DateTime.UtcNow.Date,
            ManualCost: 99999.99m,
            ServiceTypeId: serviceType.Id);

        // Act
        var response = await Client.PutAsJsonAsync(
            string.Format(ApiV1Definitions.Services.UpdateById, vehicle.Id, serviceRecord.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        result.ShouldNotBeNull();
        result.Mileage.ShouldBe(999999);
        result.TotalCost.ShouldBe(99999.99m);
    }

    #endregion

    #region Helper Methods

    private static ServiceRecordUpdateRequest CreateValidUpdateRequest(
        Guid? serviceTypeId = null,
        string title = "Updated Service")
    {
        return new ServiceRecordUpdateRequest(
            Title: title,
            Notes: "Updated maintenance notes",
            Mileage: 20000,
            ServiceDate: DateTime.UtcNow.AddDays(-1).Date,
            ManualCost: 175.00m,
            ServiceTypeId: serviceTypeId ?? Guid.NewGuid());
    }

    private async Task<ServiceRecord> CreateServiceRecordAsync(
        Guid vehicleId,
        Guid serviceTypeId,
        string title = "Original Service",
        string? notes = "Original notes",
        int? mileage = 15000,
        decimal? manualCost = 150.00m)
    {
        var serviceRecord = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            TypeId = serviceTypeId,
            Title = title,
            Notes = notes,
            Mileage = mileage,
            ServiceDate = DateTime.UtcNow.AddDays(-30).Date,
            ManualCost = manualCost
        };

        DbContext.ServiceRecords.Add(serviceRecord);
        await DbContext.SaveChangesAsync();
        return serviceRecord;
    }

    private async Task CreateServiceItemAsync(Guid serviceRecordId,
        string name,
        decimal unitPrice = 10.00m,
        decimal quantity = 1)
    {
        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecordId,
            Name = name,
            Type = Domain.Enums.Services.ServiceItemType.Part,
            UnitPrice = unitPrice,
            Quantity = quantity,
            PartNumber = null,
            Notes = null
        };

        DbContext.ServiceItems.Add(serviceItem);
        await DbContext.SaveChangesAsync();
    }

    #endregion
}