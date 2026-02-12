using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using ApiIntegrationTests.Extensions;
using Application.Features.ServiceRecords;
using Application.Features.ServiceTypes;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

[Collection("CreateServiceTypeTests")]
public class CreateServiceTypeEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateServiceType_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var request = new ServiceTypeCreateRequest("Maintenance");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateServiceType_ValidData_ReturnsCreatedWithLocationAndDto()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeCreateRequest("Oil Change");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var result = await response.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Oil Change");

        // Verify in database
        var createdInDb = await DbContext.ServiceTypes.AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == result.Id);

        createdInDb.ShouldNotBeNull();
        createdInDb.Name.ShouldBe("Oil Change");
    }

    [Fact]
    public async Task CreateServiceType_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeCreateRequest(string.Empty);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldContainOnlyError(ServiceTypeErrors.NameRequired);
    }

    [Fact]
    public async Task CreateServiceType_NameTooLong_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeCreateRequest(new string('A', 65));

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.ShouldContainOnlyError(ServiceTypeErrors.NameTooLong(64));
    }

    [Fact]
    public async Task CreateServiceType_WhitespaceName_ReturnsBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeCreateRequest("   ");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);

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
    public async Task CreateServiceType_ValidNames_ReturnsCreated(string validName)
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var request = new ServiceTypeCreateRequest(validName);

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Name.ShouldBe(validName);
    }

    [Fact]
    public async Task CreateServiceType_MultipleServiceTypes_AllPersisted()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var names = new[]
        {
            "Oil Change", "Brake Service", "Tire Rotation"
        };

        // Act
        foreach (var name in names)
        {
            var request = new ServiceTypeCreateRequest(name);
            var response = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, request);
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Assert
        var allTypes = await DbContext.ServiceTypes.AsNoTracking().ToListAsync();
        allTypes.Count.ShouldBeGreaterThanOrEqualTo(3);

        foreach (var name in names)
        {
            allTypes.ShouldContain(st => st.Name == name);
        }
    }
}