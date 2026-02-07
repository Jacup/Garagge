using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Domain.Enums.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords;

public class DeleteServiceRecordEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    #region Authentication & Authorization Tests

    [Fact]
    public async Task DeleteServiceRecord_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var serviceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicleId, serviceRecordId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteServiceRecord_VehicleDoesNotBelongToUser_ReturnsNotFound()
    {
        // Arrange
        User owner = await CreateUserAsync("owner@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(owner);
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Verify service record was not deleted
        var recordStillExists = await DbContext.ServiceRecords.AnyAsync(sr => sr.Id == serviceRecord.Id);
        recordStillExists.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteServiceRecord_VehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var serviceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(nonExistentVehicleId, serviceRecordId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceRecord_ServiceRecordDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        var nonExistentServiceRecordId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, nonExistentServiceRecordId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceRecord_ServiceRecordBelongsToDifferentVehicle_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle1 = await CreateVehicleAsync(user);
        Vehicle vehicle2 = await CreateVehicleAsync(user, "Honda", "Civic");
        ServiceType serviceType = await CreateServiceTypeAsync();
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle1.Id, serviceType.Id);

        // Act - trying to delete service record using the wrong vehicle ID
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle2.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Verify service record was not deleted
        var recordStillExists = await DbContext.ServiceRecords.AnyAsync(sr => sr.Id == serviceRecord.Id);
        recordStillExists.ShouldBeTrue();
    }

    #endregion

    #region Happy Path Tests

    [Fact]
    public async Task DeleteServiceRecord_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteServiceRecord_ValidRequest_DeletesRecordFromDatabase()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Regular Maintenance");

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify deletion
        var deletedRecord = await DbContext.ServiceRecords
            .FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteServiceRecord_WithServiceItems_DeletesRecordAndItems()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        
        var item1 = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecord.Id,
            Name = "Oil Filter",
            Type = ServiceItemType.Part,
            UnitPrice = 15.99m,
            Quantity = 1
        };
        var item2 = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecord.Id,
            Name = "Engine Oil",
            Type = ServiceItemType.Part,
            UnitPrice = 45.00m,
            Quantity = 5
        };

        DbContext.ServiceItems.AddRange(item1, item2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.DeleteAsync(
            ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
        
        var items = await DbContext.ServiceItems
            .Where(si => si.ServiceRecordId == serviceRecord.Id)
            .ToListAsync();
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task DeleteServiceRecord_WithMileage_DeletesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Inspection");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Annual Inspection",
            mileage: 50000);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteServiceRecord_WithManualCost_DeletesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Repair");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Engine Repair",
            cost: 1250.50m);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteServiceRecord_WithNotes_DeletesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Custom");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            "Custom Service",
            notes: "Important notes about this service");

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    #endregion

    #region Edge Cases & Business Logic Tests

    [Fact]
    public async Task DeleteServiceRecord_OneOfMultipleRecords_DeletesOnlySpecified()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();

        var serviceRecord1 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Record 1");
        var serviceRecord2 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Record 2");
        var serviceRecord3 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Record 3");

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord2.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord2.Id);
        deletedRecord.ShouldBeNull();
        
        var remainingRecords = await DbContext.ServiceRecords
            .Where(sr => sr.VehicleId == vehicle.Id)
            .ToListAsync();
        remainingRecords.Count.ShouldBe(2);
        remainingRecords.ShouldContain(sr => sr.Id == serviceRecord1.Id);
        remainingRecords.ShouldContain(sr => sr.Id == serviceRecord3.Id);
    }

    [Fact]
    public async Task DeleteServiceRecord_DifferentServiceTypes_DeletesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType oilChangeType = await CreateServiceTypeAsync("Oil Change");
        ServiceType tireRotationType = await CreateServiceTypeAsync("Tire Rotation");

        var oilChangeRecord = await CreateServiceRecordAsync(vehicle.Id, oilChangeType.Id);
        var tireRotationRecord = await CreateServiceRecordAsync(vehicle.Id, tireRotationType.Id);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, oilChangeRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == oilChangeRecord.Id);
        deletedRecord.ShouldBeNull();
        
        var remainingRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == tireRotationRecord.Id);
        remainingRecord.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteServiceRecord_MultipleVehicles_DeletesOnlyFromSpecifiedVehicle()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle1 = await CreateVehicleAsync(user);
        Vehicle vehicle2 = await CreateVehicleAsync(user, "Honda", "Civic");
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");

        var record1 = await CreateServiceRecordAsync(vehicle1.Id, serviceType.Id, "Vehicle 1 Record");
        var record2 = await CreateServiceRecordAsync(vehicle2.Id, serviceType.Id, "Vehicle 2 Record");

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle1.Id, record1.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify record 1 deleted
        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == record1.Id);
        deletedRecord.ShouldBeNull();

        // Verify record 2 still exists
        var remainingRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == record2.Id);
        remainingRecord.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteServiceRecord_OldServiceDate_DeletesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Historical Service");

        var oldServiceDate = new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            serviceDate: oldServiceDate);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteServiceRecord_RecentServiceDate_DeletesSuccessfully()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Recent Service");

        var recentServiceDate = DateTime.UtcNow.AddDays(-1);
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            serviceDate: recentServiceDate);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deletedRecord = await DbContext.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task DeleteServiceRecord_DeleteSameRecordTwice_SecondReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Test Service");
        ServiceRecord serviceRecord = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        var url = ApiV1Definitions.Services.DeleteById.WithIds(vehicle.Id, serviceRecord.Id);

        // Act - First deletion
        var firstResponse = await Client.DeleteAsync(url);

        // Assert first deletion
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act - Second deletion (idempotency test)
        var secondResponse = await Client.DeleteAsync(url);

        // Assert second deletion
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion
}