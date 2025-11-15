using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords;

public class GetServiceRecordByIdEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    #region Authentication & Authorization Tests

    [Fact]
    public async Task GetServiceRecordById_Unauthorized_UserNotAuthenticated()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var serviceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicleId, serviceRecordId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServiceRecordById_Unauthorized_VehicleDoesNotBelongToUser()
    {
        // Arrange
        User owner = await CreateUserAsync("owner@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(owner);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServiceRecordById_VehicleDoesNotExist_NotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var serviceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(nonExistentVehicleId, serviceRecordId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServiceRecordById_ServiceRecordDoesNotExist_NotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        var nonExistentServiceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, nonExistentServiceRecordId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServiceRecordById_ServiceRecordBelongsToDifferentVehicle_NotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle1 = await CreateVehicleAsync(user);
        Vehicle vehicle2 = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle1.Id, serviceType.Id);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle2.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServiceRecordById_UserOwnsVehicleAndServiceRecordExists_Ok()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Basic Functionality Tests

    [Fact]
    public async Task GetServiceRecordById_WithAllProperties_ReturnsCorrectDtoStructure()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");
        var serviceDate = new DateTime(2024, 5, 15, 10, 30, 0, DateTimeKind.Utc);

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Premium Oil Change",
            serviceDate,
            15000,
            150.75m,
            "Used synthetic oil");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(serviceRecord.Id);
        dto.Title.ShouldBe("Premium Oil Change");
        dto.Notes.ShouldBe("Used synthetic oil");
        dto.Mileage.ShouldBe(15000);
        dto.ServiceDate.ShouldBe(serviceDate);
        dto.TotalCost.ShouldBe(150.75m);
        dto.TypeId.ShouldBe(serviceType.Id);
        dto.Type.ShouldBe("Oil Change");
        dto.VehicleId.ShouldBe(vehicle.Id);
        dto.CreatedDate.ShouldNotBe(default);
        dto.UpdatedDate.ShouldNotBe(default);
        dto.ServiceItems.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetServiceRecordById_WithMinimalData_ReturnsServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Basic Service");
        var serviceDate = new DateTime(2024, 6, 1, 14, 0, 0, DateTimeKind.Utc);

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Basic Inspection",
            serviceDate); // No notes

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(serviceRecord.Id);
        dto.Title.ShouldBe("Basic Inspection");
        dto.Notes.ShouldBeNull();
        dto.Mileage.ShouldBeNull();
        dto.ServiceDate.ShouldBe(serviceDate);
        dto.TotalCost.ShouldBe(0m);
        dto.TypeId.ShouldBe(serviceType.Id);
        dto.Type.ShouldBe("Basic Service");
        dto.VehicleId.ShouldBe(vehicle.Id);
    }

    [Fact]
    public async Task GetServiceRecordById_WithServiceItems_ReturnsServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Brake Service");
        var serviceDate = new DateTime(2024, 7, 10, 9, 0, 0, DateTimeKind.Utc);

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Front Brake Replacement",
            serviceDate,
            25000,
            null,
            "Front brake pads and rotors replaced");

        await CreateServiceItemAsync(serviceRecord.Id, "Brake Pads", 100m, 2);
        await CreateServiceItemAsync(serviceRecord.Id, "Brake Rotors", 150m, 2);
        await CreateServiceItemAsync(serviceRecord.Id, "Labor", 80m);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(serviceRecord.Id);
        dto.ServiceItems.ShouldNotBeNull();
        dto.ServiceItems.Count.ShouldBe(3);
        dto.TotalCost.ShouldBe(580m); // (100*2) + (150*2) + (80*1)

        var brakesPad = dto.ServiceItems.FirstOrDefault(si => si.Name == "Brake Pads");
        brakesPad.ShouldNotBeNull();
        brakesPad.UnitPrice.ShouldBe(100m);
        brakesPad.Quantity.ShouldBe(2);
        brakesPad.TotalPrice.ShouldBe(200m);
    }

    [Fact]
    public async Task GetServiceRecordById_NoItemsExist_ReturnsEmptyServiceItems()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Quick Fix");

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Minor Adjustment",
            DateTime.UtcNow,
            10000,
            25m);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.ServiceItems.ShouldNotBeNull();
        dto.ServiceItems.ShouldBeEmpty();
        dto.TotalCost.ShouldBe(25m); // ManualCost
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GetServiceRecordById_WithZeroCost_ReturnsServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Warranty Service");

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Warranty Repair",
            DateTime.UtcNow,
            18000,
            0m,
            "Covered under warranty");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.TotalCost.ShouldBe(0m);
        dto.Title.ShouldBe("Warranty Repair");
    }

    [Fact]
    public async Task GetServiceRecordById_WithHighMileage_ReturnsServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Major Service");

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "500K Mile Service",
            DateTime.UtcNow,
            500000,
            2500m,
            "High mileage maintenance");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.Mileage.ShouldBe(500000);
        dto.TotalCost.ShouldBe(2500m);
    }

    [Fact]
    public async Task GetServiceRecordById_WithPastDate_ReturnsServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Historical Service");
        var pastDate = new DateTime(2020, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Historical Maintenance",
            pastDate,
            5000,
            100m,
            "Service from 2020");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.ServiceDate.ShouldBe(pastDate);
        dto.Title.ShouldBe("Historical Maintenance");
    }

    [Fact]
    public async Task GetServiceRecordById_WithLongNotes_ReturnsServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Detailed Service");
        var longNotes = new string('A', 500);

        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Comprehensive Service",
            DateTime.UtcNow,
            30000,
            350m,
            longNotes);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.Notes.ShouldNotBeNull();
        dto.Notes.ShouldBe(longNotes);
        dto.Notes.Length.ShouldBe(500);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GetServiceRecordById_VehicleIdIsInvalid_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var invalidVehicleId = "invalid-guid";
        var serviceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetById.Replace("{0}", invalidVehicleId).Replace("{1}", serviceRecordId.ToString())}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServiceRecordById_ServiceRecordIdIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var vehicleId = Guid.NewGuid();
        var invalidServiceRecordId = "invalid-guid";

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetById.Replace("{0}", vehicleId.ToString()).Replace("{1}", invalidServiceRecordId)}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region Multiple Vehicles Tests

    [Fact]
    public async Task GetServiceRecordById_UserHasMultipleVehicles_ReturnsCorrectServiceRecord()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");

        Vehicle vehicle1 = await CreateVehicleAsync(user, "Vehicle 1");
        Vehicle vehicle2 = await CreateVehicleAsync(user, "Vehicle 2");

        ServiceType serviceType = await CreateServiceTypeAsync();

        ServiceRecord serviceRecord1 = await CreateServiceRecordAsync(vehicle1.Id, serviceType.Id, "Service for Vehicle 1");
        await CreateServiceRecordAsync(vehicle2.Id, serviceType.Id, "Service for Vehicle 2");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle1.Id, serviceRecord1.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);

        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(serviceRecord1.Id);
        dto.Title.ShouldBe("Service for Vehicle 1");
        dto.VehicleId.ShouldBe(vehicle1.Id);
    }

    [Fact]
    public async Task GetServiceRecordById_RequestingServiceRecordFromWrongVehicle_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");

        Vehicle vehicle1 = await CreateVehicleAsync(user, "Vehicle 1");
        Vehicle vehicle2 = await CreateVehicleAsync(user, "Vehicle 2");

        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord1 = await CreateServiceRecordAsync(vehicle1.Id, serviceType.Id);

        // Act - Try to get vehicle1's service record using vehicle2's ID
        var response = await Client.GetAsync(ApiV1Definition.Services.GetById.WithIds(vehicle2.Id, serviceRecord1.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion
}