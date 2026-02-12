using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Fixtures;
using ApiIntegrationTests.Extensions;
using Application.Features.ServiceTypes;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

[Collection("DeleteServiceTypeTests")]
public class DeleteServiceTypeEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteServiceType_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var serviceType = await CreateServiceTypeAsync();

        // Act
        var response = await Client.DeleteAsync(
            ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteServiceType_ValidId_ReturnsNoContent()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("To Delete");

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify deleted from database
        var deletedFromDb = await DbContext.ServiceTypes.AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == serviceType.Id);

        deletedFromDb.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteServiceType_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(nonExistentId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(ServiceTypeErrors.NotFound);
    }

    [Fact]
    public async Task DeleteServiceType_EmptyGuid_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(Guid.Empty));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceType_InvalidGuidFormat_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.DeleteAsync("/api/vehicles/service-records/types/invalid-guid");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceType_DeleteTwice_SecondReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("To Delete");

        // Act - First delete
        var response1 = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        // Act - Second delete
        var response2 = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response2.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteServiceType_UsedInServiceRecords_ReturnsConflict()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var vehicle = await CreateVehicleAsync();
        var serviceType = await CreateServiceTypeAsync("Oil Change");

        // Create a service record using this type
        await CreateServiceRecordAsync(
            vehicle.Id,
            serviceType.Id,
            title: "Test Service");

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(ServiceTypeErrors.ServiceRecordsExists);

        // Verify NOT deleted from database
        var stillInDb = await DbContext.ServiceTypes.AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == serviceType.Id);

        stillInDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteServiceType_MultipleTypes_OnlySpecifiedDeleted()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var type1 = await CreateServiceTypeAsync("Type 1");
        var type2 = await CreateServiceTypeAsync("Type 2");
        var type3 = await CreateServiceTypeAsync("Type 3");

        // Act - Delete only type2
        var response = await Client.DeleteAsync(ApiV1Definitions.ServiceTypes.Delete.WithId(type2.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var type1InDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(st => st.Id == type1.Id);

        var type2InDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(st => st.Id == type2.Id);

        var type3InDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(st => st.Id == type3.Id);

        type1InDb.ShouldNotBeNull();
        type2InDb.ShouldBeNull();
        type3InDb.ShouldNotBeNull();
    }
}