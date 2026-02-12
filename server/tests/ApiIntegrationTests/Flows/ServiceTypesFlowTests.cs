using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Application.Features.ServiceRecords;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Flows;

[Collection("ServiceTypeFlowTests")]
public class ServiceTypeFlowTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    #region Complete CRUD Flow Tests

    [Fact]
    public async Task ServiceTypeFlow_CreateGetUpdateDelete_Success()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // CREATE - Add new service type
        var createRequest = new ServiceTypeCreateRequest("Oil Change");
        var createResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, createRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdType = await createResponse.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        createdType.ShouldNotBeNull();
        createdType.Name.ShouldBe("Oil Change");

        // GET ALL - Verify it appears in list
        var getAllResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        getAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var allTypes = await getAllResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypes.ShouldNotBeNull();
        allTypes.Count.ShouldBe(1);
        allTypes[0].Name.ShouldBe("Oil Change");

        // UPDATE - Change the name
        var updateRequest = new ServiceTypeUpdateRequest("Premium Oil Change");
        var updateResponse = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(createdType.Id),
            updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updatedType = await updateResponse.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        updatedType.ShouldNotBeNull();
        updatedType.Name.ShouldBe("Premium Oil Change");

        // GET ALL - Verify update persisted
        var getAllAfterUpdateResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypesAfterUpdate = await getAllAfterUpdateResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypesAfterUpdate.ShouldNotBeNull();
        allTypesAfterUpdate.Count.ShouldBe(1);
        allTypesAfterUpdate[0].Name.ShouldBe("Premium Oil Change");

        // DELETE - Remove the service type
        var deleteResponse = await Client.DeleteAsync(
            ApiV1Definitions.ServiceTypes.Delete.WithId(createdType.Id));

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // GET ALL - Verify it's gone
        var getAllAfterDeleteResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypesAfterDelete = await getAllAfterDeleteResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypesAfterDelete.ShouldNotBeNull();
        allTypesAfterDelete.ShouldBeEmpty();
    }

    [Fact]
    public async Task ServiceTypeFlow_CreateMultipleTypesGetAllUpdateOne_Success()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // CREATE - Add three different types
        var types = new[]
        {
            "Oil Change", "Brake Service", "Tire Rotation"
        };

        var createdIds = new List<Guid>();

        foreach (var typeName in types)
        {
            var createRequest = new ServiceTypeCreateRequest(typeName);
            var createResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, createRequest);
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

            var createdType = await createResponse.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
            createdType.ShouldNotBeNull();
            createdIds.Add(createdType.Id);
        }

        // GET ALL - Verify all three exist
        var getAllResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypes = await getAllResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypes.ShouldNotBeNull();
        allTypes.Count.ShouldBe(3);

        foreach (var typeName in types)
        {
            allTypes.ShouldContain(st => st.Name == typeName);
        }

        // UPDATE - Change only the second one
        var updateRequest = new ServiceTypeUpdateRequest("Advanced Brake Service");
        var updateResponse = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(createdIds[1]),
            updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // GET ALL - Verify only second one changed
        var getAllAfterUpdateResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypesAfterUpdate = await getAllAfterUpdateResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypesAfterUpdate.ShouldNotBeNull();
        allTypesAfterUpdate.Count.ShouldBe(3);
        allTypesAfterUpdate.ShouldContain(st => st.Name == "Oil Change");
        allTypesAfterUpdate.ShouldContain(st => st.Name == "Advanced Brake Service");
        allTypesAfterUpdate.ShouldContain(st => st.Name == "Tire Rotation");
        allTypesAfterUpdate.ShouldNotContain(st => st.Name == "Brake Service");
    }

    #endregion

    #region Service Type with Service Records Flow

    [Fact]
    public async Task ServiceTypeFlow_CreateTypeUseInServiceRecordDeleteType_DeleteFails()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var vehicle = await CreateVehicleAsync();

        // CREATE - Service type
        var createTypeRequest = new ServiceTypeCreateRequest("Oil Change");
        var createTypeResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, createTypeRequest);
        var serviceType = await createTypeResponse.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        serviceType.ShouldNotBeNull();

        // CREATE - Service record using this type
        var createRecordRequest = new ServiceRecordCreateRequest(
            Title: "First Oil Change",
            Notes: "Initial service",
            Mileage: 5000,
            ServiceDate: DateTime.UtcNow.AddDays(-1),
            ManualCost: 50.00m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        var createRecordResponse = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
            createRecordRequest);

        createRecordResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // DELETE - Try to delete service type (should fail - in use)
        var deleteResponse = await Client.DeleteAsync(
            ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // GET ALL - Verify service type still exists
        var getAllResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypes = await getAllResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypes.ShouldNotBeNull();
        allTypes.Count.ShouldBe(1);
        allTypes[0].Name.ShouldBe("Oil Change");
    }

    [Fact]
    public async Task ServiceTypeFlow_CreateTypeUseInRecordDeleteRecordThenDeleteType_Success()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var vehicle = await CreateVehicleAsync();

        // CREATE - Service type
        var createTypeRequest = new ServiceTypeCreateRequest("Oil Change");
        var createTypeResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, createTypeRequest);
        var serviceType = await createTypeResponse.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        serviceType.ShouldNotBeNull();

        // CREATE - Service record using this type
        var createRecordRequest = new ServiceRecordCreateRequest(
            Title: "First Oil Change",
            Notes: "Initial service",
            Mileage: 5000,
            ServiceDate: DateTime.UtcNow.AddDays(-1),
            ManualCost: 50.00m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        var createRecordResponse = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
            createRecordRequest);

        var serviceRecord = await createRecordResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
        serviceRecord.ShouldNotBeNull();

        // DELETE - Service record first
        var deleteRecordResponse = await Client.DeleteAsync(
            string.Format(ApiV1Definitions.Services.DeleteById, vehicle.Id, serviceRecord.Id));

        deleteRecordResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // DELETE - Now delete service type (should succeed)
        var deleteTypeResponse = await Client.DeleteAsync(
            ApiV1Definitions.ServiceTypes.Delete.WithId(serviceType.Id));

        deleteTypeResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // GET ALL - Verify service type is gone
        var getAllResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypes = await getAllResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypes.ShouldNotBeNull();
        allTypes.ShouldBeEmpty();
    }

    [Fact]
    public async Task ServiceTypeFlow_UpdateTypeNameUsedInServiceRecord_RecordReflectsChange()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var vehicle = await CreateVehicleAsync();

        // CREATE - Service type
        var createTypeRequest = new ServiceTypeCreateRequest("Oil Change");
        var createTypeResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, createTypeRequest);
        var serviceType = await createTypeResponse.Content.ReadFromJsonAsync<ServiceTypeDto>(DefaultJsonSerializerOptions);
        serviceType.ShouldNotBeNull();

        // CREATE - Service record using this type
        var createRecordRequest = new ServiceRecordCreateRequest(
            Title: "First Oil Change",
            Notes: "Initial service",
            Mileage: 5000,
            ServiceDate: DateTime.UtcNow.AddDays(-1),
            ManualCost: 50.00m,
            ServiceTypeId: serviceType.Id,
            ServiceItems: new List<ServiceItemCreateRequest>());

        var createRecordResponse = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.Services.Create, vehicle.Id),
            createRecordRequest);

        var serviceRecord = await createRecordResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
        serviceRecord.ShouldNotBeNull();
        serviceRecord.Type.ShouldBe("Oil Change");

        // UPDATE - Service type name
        var updateTypeRequest = new ServiceTypeUpdateRequest("Premium Oil Change");
        var updateTypeResponse = await Client.PutAsJsonAsync(
            ApiV1Definitions.ServiceTypes.Update.WithId(serviceType.Id),
            updateTypeRequest);

        updateTypeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // GET - Service record should reflect new type name
        var getRecordResponse = await Client.GetAsync(
            string.Format(ApiV1Definitions.Services.GetById, vehicle.Id, serviceRecord.Id));

        var updatedRecord = await getRecordResponse.Content.ReadFromJsonAsync<ServiceRecordDto>(DefaultJsonSerializerOptions);
        updatedRecord.ShouldNotBeNull();
        updatedRecord.Type.ShouldBe("Premium Oil Change");
    }

    #endregion

    #region Empty State and Validation Flow

    [Fact]
    public async Task ServiceTypeFlow_GetAllBeforeCreate_ReturnsEmptyList()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // Act - Get all types before creating any
        var getAllResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);

        // Assert
        getAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var allTypes = await getAllResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypes.ShouldNotBeNull();
        allTypes.ShouldBeEmpty();
    }

    [Fact]
    public async Task ServiceTypeFlow_CreateInvalidThenValid_OnlyValidPersists()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        // CREATE - Invalid (empty name)
        var invalidRequest = new ServiceTypeCreateRequest(string.Empty);
        var invalidResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, invalidRequest);
        invalidResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // CREATE - Valid
        var validRequest = new ServiceTypeCreateRequest("Oil Change");
        var validResponse = await Client.PostAsJsonAsync(ApiV1Definitions.ServiceTypes.Create, validRequest);
        validResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // GET ALL - Only valid one exists
        var getAllResponse = await Client.GetAsync(ApiV1Definitions.ServiceTypes.GetAll);
        var allTypes = await getAllResponse.Content.ReadFromJsonAsync<List<ServiceTypeDto>>(DefaultJsonSerializerOptions);
        allTypes.ShouldNotBeNull();
        allTypes.Count.ShouldBe(1);
        allTypes[0].Name.ShouldBe("Oil Change");
    }

    #endregion
}