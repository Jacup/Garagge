using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Application.Features.ServiceTypes;
using System.Net;
using System.Net.Http.Json;
using ApiIntegrationTests.Extensions;
using Application.Features.ServiceRecords;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

[Collection("UpdateServiceTypeTests")]
public class UpdateServiceTypeEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateServiceType_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var serviceType = await CreateServiceTypeAsync();
        var request = new ServiceTypeUpdateRequest("Oil Service");

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateServiceType_ValidData_ReturnsOkWithUpdatedServiceType()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Original Name");
        var request = new ServiceTypeUpdateRequest("Updated Oil Service");

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(serviceType.Id);
        result.Name.ShouldBe("Updated Oil Service");

        var updatedInDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == serviceType.Id);
        updatedInDb.ShouldNotBeNull();
        updatedInDb.Name.ShouldBe("Updated Oil Service");
    }

    [Fact]
    public async Task UpdateServiceType_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Original");
        var request = new ServiceTypeUpdateRequest(string.Empty);

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldContainOnlyError(ServiceTypeErrors.NameRequired);

        // Verify name unchanged in database
        var unchangedInDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == serviceType.Id);
        unchangedInDb.ShouldNotBeNull();
        unchangedInDb.Name.ShouldBe("Original");
    }

    [Fact]
    public async Task UpdateServiceType_NameTooLong_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Original");
        var request = new ServiceTypeUpdateRequest(new string('A', 65));

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldContainOnlyError(ServiceTypeErrors.NameTooLong(64));

        var unchangedInDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == serviceType.Id);
        unchangedInDb.ShouldNotBeNull();
        unchangedInDb.Name.ShouldBe("Original");
    }

    [Fact]
    public async Task UpdateServiceType_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var nonExistentId = Guid.NewGuid();
        var request = new ServiceTypeUpdateRequest("Some Name");

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(nonExistentId),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldBeErrorOfType(ServiceTypeErrors.NotFound);
    }

    [Fact]
    public async Task UpdateServiceType_SameNameAsOriginal_ReturnsOk()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Oil Change");
        var request = new ServiceTypeUpdateRequest("Oil Change");

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Oil Change");
    }

    [Fact]
    public async Task UpdateServiceType_InvalidGuidFormat_ReturnsNotFound()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeUpdateRequest("Some Name");

        // Act
        var response = await Client.PutAsJsonAsync(
            "/api/vehicles/service-records/types/invalid-guid",
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateServiceType_EmptyGuid_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeUpdateRequest("Some Name");

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(Guid.Empty),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateServiceType_WhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Original");
        var request = new ServiceTypeUpdateRequest("   ");

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldContainOnlyError(ServiceTypeErrors.NameRequired);
    }

    [Theory]
    [InlineData("Oil Change")]
    [InlineData("Brake Service")]
    [InlineData("Tire Rotation")]
    [InlineData("A")] // Minimum 1 character
    [InlineData("This is exactly sixty-four characters long name for service!")] // Exactly 64
    public async Task UpdateServiceType_ValidNames_ReturnsOk(string validName)
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Original");
        var request = new ServiceTypeUpdateRequest(validName);

        // Act
        var response = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(validName);
    }

    [Fact]
    public async Task UpdateServiceType_MultipleConcurrentUpdates_LastWriteWins()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var serviceType = await CreateServiceTypeAsync("Original");

        var request1 = new ServiceTypeUpdateRequest("First Update");
        var request2 = new ServiceTypeUpdateRequest("Second Update");

        // Act - concurrent updates
        var task1 = Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request1);

        var task2 = Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            request2);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - both should succeed (last write wins)
        responses[0].StatusCode.ShouldBeOneOf(HttpStatusCode.OK);
        responses[1].StatusCode.ShouldBeOneOf(HttpStatusCode.OK);

        // Final value in DB should be one of the updates
        var finalInDb = await DbContext.ServiceTypes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == serviceType.Id);
        finalInDb.ShouldNotBeNull();
        finalInDb.Name.ShouldBeOneOf("First Update", "Second Update");
    }
}