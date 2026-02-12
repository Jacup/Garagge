using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Fixtures;
using Application.Features.ServiceRecords;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

[Collection("GetServiceTypesTests")]
public class GetServiceRecordTypesEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetServiceRecordTypes_UserNotAuthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServiceRecordTypes_NoTypesExist_ReturnsEmptyList()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetServiceRecordTypes_TypesExist_ReturnsAllTypes()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var type1 = await CreateServiceTypeAsync("Oil Change");
        var type2 = await CreateServiceTypeAsync("Brake Service");
        var type3 = await CreateServiceTypeAsync("Tire Rotation");

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        result.ShouldContain(st => st.Id == type1.Id && st.Name == "Oil Change");
        result.ShouldContain(st => st.Id == type2.Id && st.Name == "Brake Service");
        result.ShouldContain(st => st.Id == type3.Id && st.Name == "Tire Rotation");
    }

    [Fact]
    public async Task GetServiceRecordTypes_MultipleCalls_ReturnsSameData()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        await CreateServiceTypeAsync("Oil Change");
        await CreateServiceTypeAsync("Brake Service");

        // Act
        var response1 = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var response2 = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);

        // Assert
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);
        response2.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result1 = await response1.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        var result2 = await response2.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);

        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result1.Count.ShouldBe(result2.Count);
        result1.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetServiceRecordTypes_OrderedByName_ReturnsAlphabetically()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        await CreateServiceTypeAsync("Zulu Service");
        await CreateServiceTypeAsync("Alpha Service");
        await CreateServiceTypeAsync("Mike Service");

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }
}