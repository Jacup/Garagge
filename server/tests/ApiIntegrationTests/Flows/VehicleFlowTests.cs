using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.Core;
using Application.Vehicles;
using Domain.Entities.Vehicles;
using Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Flows;

[Collection("VehicleFlowTests")]
public class VehicleFlowTests : BaseIntegrationTest
{
    public VehicleFlowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task VehicleLifecycleFlow_CreateUpdateDeleteVehicle_Success()
    {
        await CreateAndAuthenticateUser();

        const string initialBrand = "Toyota";
        const string initialModel = "Corolla";
        const EngineType initialEngineType = EngineType.Fuel;
        const int initialYear = 2019;
        const VehicleType initialVehicleType = VehicleType.Bus;
        const string initialVin = "1FA6P8CF8GH123AAA";

        // Create Vehicle
        var createVehicleCommand = new CreateVehicleRequest(initialBrand, initialModel, initialEngineType, initialYear, initialVehicleType, initialVin, []);

        var createResponse = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, createVehicleCommand);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadFromJsonAsync<VehicleDto>(DefaultJsonSerializerOptions);
        createContent.ShouldNotBeNull();
        createContent.Brand.ShouldBe(initialBrand);
        createContent.Model.ShouldBe(initialModel);
        createContent.EngineType.ShouldBe(initialEngineType);
        createContent.ManufacturedYear.ShouldBe(initialYear);
        createContent.Type.ShouldBe(initialVehicleType);
        createContent.VIN.ShouldBe(initialVin);

        // Get Vehicle
        var getResponse = await Client.GetAsync(string.Format(ApiV1Definition.Vehicles.GetById, createContent.Id));
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadFromJsonAsync<VehicleDto>(DefaultJsonSerializerOptions);
        getContent.ShouldNotBeNull();
        getContent.Brand.ShouldBe(initialBrand);
        getContent.Model.ShouldBe(initialModel);
        getContent.EngineType.ShouldBe(initialEngineType);
        getContent.ManufacturedYear.ShouldBe(initialYear);
        getContent.Type.ShouldBe(initialVehicleType);
        getContent.VIN.ShouldBe(initialVin);


        // Update Vehicle
        const string updatedBrand = "Lexus";
        const string updatedModel = "IS";
        const EngineType updatedEngineType = EngineType.Hybrid;
        const int updatedYear = 2020;
        const VehicleType updatedVehicleType = VehicleType.Car;
        const string updatedVin = "1FA6P8CF8GH123456";

        var updateVehicleRequest = new UpdateVehicleRequest(updatedBrand, updatedModel, updatedEngineType, updatedYear, updatedVehicleType, updatedVin);
        var updateResponse = await Client.PutAsJsonAsync(string.Format(ApiV1Definition.Vehicles.UpdateById, createContent.Id), updateVehicleRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<VehicleDto>(DefaultJsonSerializerOptions);
        updateResult.ShouldNotBeNull();
        updateResult.Brand.ShouldBe(updatedBrand);
        updateResult.Model.ShouldBe(updatedModel);
        updateResult.EngineType.ShouldBe(updatedEngineType);
        updateResult.ManufacturedYear.ShouldBe(updatedYear);
        updateResult.Type.ShouldBe(updatedVehicleType);
        updateResult.VIN.ShouldBe(updatedVin);

        // Get Vehicle After Update
        var getAfterUpdateResponse = await Client.GetAsync(string.Format(ApiV1Definition.Vehicles.GetById, createContent.Id));
        getAfterUpdateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getAfterUpdateContent = await getAfterUpdateResponse.Content.ReadFromJsonAsync<VehicleDto>(DefaultJsonSerializerOptions);
        getAfterUpdateContent.ShouldNotBeNull();
        getAfterUpdateContent.Brand.ShouldBe(updatedBrand);
        getAfterUpdateContent.Model.ShouldBe(updatedModel);
        getAfterUpdateContent.EngineType.ShouldBe(updatedEngineType);
        getAfterUpdateContent.ManufacturedYear.ShouldBe(updatedYear);
        getAfterUpdateContent.Type.ShouldBe(updatedVehicleType);
        getAfterUpdateContent.VIN.ShouldBe(updatedVin);

        // Delete Vehicle
        var deleteResponse = await Client.DeleteAsync(string.Format(ApiV1Definition.Vehicles.DeleteById, createContent.Id));
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Get Vehicle After Delete
        var getAfterDeleteResponse = await Client.GetAsync(string.Format(ApiV1Definition.Vehicles.GetById, createContent.Id));
        getAfterDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VehicleCollectionFlow_CreateMultipleVehiclesAndRetrieve_Success()
    {
        await CreateAndAuthenticateUser();

        for (int i = 0; i < 15; i++)
        {
            var command = new CreateVehicleRequest($"TestBrand X{i}", $"TestModel Y{i}", EngineType.Fuel, EnergyTypes: []);
            var response = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, command);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Get All Vehicles - First Page
        var getAllResponse = await Client.GetAsync(ApiV1Definition.Vehicles.GetAll);
        getAllResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pagedListOfVehicles = await getAllResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);

        pagedListOfVehicles.ShouldNotBeNull();
        pagedListOfVehicles.Items.Count.ShouldBe(10);
        pagedListOfVehicles.PageSize.ShouldBe(10);
        pagedListOfVehicles.TotalCount.ShouldBe(15);
        pagedListOfVehicles.HasNextPage.ShouldBeTrue();
        pagedListOfVehicles.HasPreviousPage.ShouldBeFalse();

        // Get All Vehicles - Second Page
        var getSecondPageResponse = await Client.GetAsync($"{ApiV1Definition.Vehicles.GetAll}?page=2&pageSize=10");
        getSecondPageResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var secondPage = await getSecondPageResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);

        secondPage.ShouldNotBeNull();
        secondPage.Items.Count.ShouldBe(5);
        secondPage.PageSize.ShouldBe(10);
        secondPage.TotalCount.ShouldBe(15);
        secondPage.HasNextPage.ShouldBeFalse();
        secondPage.HasPreviousPage.ShouldBeTrue();

        var getPageWithIncreasedPageSizeResponse = await Client.GetAsync($"{ApiV1Definition.Vehicles.GetAll}?pageSize=20");
        getPageWithIncreasedPageSizeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageWithIncreasedPageSize =
            await getPageWithIncreasedPageSizeResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);

        pageWithIncreasedPageSize.ShouldNotBeNull();
        pageWithIncreasedPageSize.Items.Count.ShouldBe(15);
        pageWithIncreasedPageSize.PageSize.ShouldBe(20);
        pageWithIncreasedPageSize.TotalCount.ShouldBe(15);
        pageWithIncreasedPageSize.HasNextPage.ShouldBeFalse();
        pageWithIncreasedPageSize.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public async Task VehicleSearchFlow_CreateVehiclesAndSearch_Success()
    {
        await CreateAndAuthenticateUser();

        List<CreateVehicleRequest> commands =
        [
            new("Toyota", "Corolla", EngineType.Fuel, EnergyTypes: []),
            new("Audi", "A4", EngineType.Fuel, EnergyTypes: []),
            new("BMW", "X5", EngineType.Fuel, EnergyTypes: []),
            new("Mercedes", "GLA", EngineType.Fuel, EnergyTypes: []),
        ];

        foreach (CreateVehicleRequest createVehicleCommand in commands)
        {
            await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, createVehicleCommand);
        }

        // Search Vehicles by Brand containing 'a' (case-insensitive)
        var searchLowercaseResponse = await Client.GetAsync($"{ApiV1Definition.Vehicles.GetAll}?searchTerm=a");
        searchLowercaseResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var searchLowercaseResult = await searchLowercaseResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);
        searchLowercaseResult.ShouldNotBeNull();
        searchLowercaseResult.Items.Count.ShouldBe(3); // Toyota, Audi, Mercedes
        searchLowercaseResult.Items.ShouldAllBe(v =>
            v.Brand.Contains('a', StringComparison.OrdinalIgnoreCase) ||
            v.Model.Contains('a', StringComparison.OrdinalIgnoreCase));

        // Search Vehicles by Model containing 'X' (case-insensitive)
        var searchUppercaseResponse = await Client.GetAsync($"{ApiV1Definition.Vehicles.GetAll}?searchTerm=X");
        searchUppercaseResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var searchUppercaseResult = await searchUppercaseResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);
        searchUppercaseResult.ShouldNotBeNull();
        searchUppercaseResult.Items.Count.ShouldBe(1); // BMW X5
        searchUppercaseResult.Items.ShouldAllBe(v => v.Model.Contains('X', StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task VehicleDataIsolationFlow_TwoUsersCreateVehicles_DataSeparation()
    {
        var user1 = await CreateUserAsync("user1@garagge.app", "password123");
        var user2 = await CreateUserAsync("user2@garagge.app", "password345");

        // Authenticate as User 1 and create a vehicle
        var loginUser1Response = await LoginUser(user1.Email, "password123");
        Authenticate(loginUser1Response.AccessToken);
        var createVehicleUser1Command = new CreateVehicleRequest("User1Brand", "User1Model", EngineType.Fuel, EnergyTypes: []);
        var createUser1Response = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, createVehicleUser1Command);
        createUser1Response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Authenticate as User 2 and create a vehicle
        var loginUser2Response = await LoginUser(user2.Email, "password345");
        Authenticate(loginUser2Response.AccessToken);
        var createVehicleUser2Command = new CreateVehicleRequest("User2Brand", "User2Model", EngineType.Fuel, EnergyTypes: []);
        var createUser2Response = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, createVehicleUser2Command);
        createUser2Response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Retrieve vehicles as User 2
        var getUser2VehiclesResponse = await Client.GetAsync(ApiV1Definition.Vehicles.GetAll);
        getUser2VehiclesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user2Vehicles = await getUser2VehiclesResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);
        user2Vehicles.ShouldNotBeNull();
        user2Vehicles.Items.Count.ShouldBe(1);
        user2Vehicles.Items[0].Brand.ShouldBe("User2Brand");

        // Retrieve vehicles as User 1
        Authenticate(loginUser1Response.AccessToken);
        var getUser1VehiclesResponse = await Client.GetAsync(ApiV1Definition.Vehicles.GetAll);
        getUser1VehiclesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user1Vehicles = await getUser1VehiclesResponse.Content.ReadFromJsonAsync<PagedList<VehicleDto>>(DefaultJsonSerializerOptions);
        user1Vehicles.ShouldNotBeNull();
        user1Vehicles.Items.Count.ShouldBe(1);
        user1Vehicles.Items[0].Brand.ShouldBe("User1Brand");
    }

    [Fact]
    public async Task VehicleAuthFlow_UnauthorizedAccess_Returns401()
    {
        var user = await CreateUserAsync();
        var vehicle = new Vehicle { Brand = "Toyota", Model = "Corolla", EngineType = EngineType.Fuel, UserId = user.Id };

        DbContext.Vehicles.Add(vehicle);
        await DbContext.SaveChangesAsync();

        var getResponse = await Client.GetAsync(string.Format(ApiV1Definition.Vehicles.GetById, vehicle.Id));
        getResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var createCommand = new CreateVehicleRequest("Honda", "Civic", EngineType.Fuel, EnergyTypes: []);
        var createResponse = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, createCommand);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var updateRequest = new UpdateVehicleRequest("Honda", "Civic", EngineType.Fuel);
        var updateResponse = await Client.PutAsJsonAsync(string.Format(ApiV1Definition.Vehicles.UpdateById, vehicle.Id), updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var deleteResponse = await Client.DeleteAsync(string.Format(ApiV1Definition.Vehicles.DeleteById, vehicle.Id));
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var getAllResponse = await Client.GetAsync(ApiV1Definition.Vehicles.GetAll);
        getAllResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task VehicleValidationFlow_InvalidCreateData_Returns400()
    {
        await CreateAndAuthenticateUser();

        var invalidCreateCommands = new List<CreateVehicleRequest>
        {
            new("", "Model", EngineType.Fuel, EnergyTypes: []), // Empty Brand
            new("Brand", "", EngineType.Fuel, EnergyTypes: []), // Empty Model
            new("Brand", "Model", (EngineType)999), // Invalid EngineType
            new("Brand", "Model", EngineType.Fuel, 1800, EnergyTypes: []), // Year too early
            new("Brand", "Model", EngineType.Fuel, 3000, EnergyTypes: []), // Year too late
            new("Brand", "Model", EngineType.Fuel, 2020, (VehicleType)999, EnergyTypes: []), // Invalid VehicleType
            new("Brand", "Model", EngineType.Fuel, 2020, VehicleType.Car, new string('A', 16), EnergyTypes: []), // VIN too short
            new("Brand", "Model", EngineType.Fuel, 2020, VehicleType.Car, new string('A', 18), EnergyTypes: []), // VIN too long
        };

        foreach (var command in invalidCreateCommands)
        {
            var response = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, command);
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task VehicleValidationFlow_InvalidUpdateData_Returns400()
    {
        await CreateAndAuthenticateUser();

        var createCommand = new CreateVehicleRequest("ValidBrand", "ValidModel", EngineType.Fuel, EnergyTypes: []);
        var createResponse = await Client.PostAsJsonAsync(ApiV1Definition.Vehicles.Create, createCommand);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        createResponse.Content.ShouldNotBeNull();

        var result = await createResponse.Content.ReadFromJsonAsync<VehicleDto>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();

        var invalidUpdateCommands = new List<UpdateVehicleRequest>
        {
            new("", "Model", EngineType.Fuel), // Empty Brand
            new("Brand", "", EngineType.Fuel), // Empty Model
            new("Brand", "Model", (EngineType)999), // Invalid EngineType
            new("Brand", "Model", EngineType.Fuel, 1800), // Year too early
            new("Brand", "Model", EngineType.Fuel, 3000), // Year too late
            new("Brand", "Model", EngineType.Fuel, 2020, (VehicleType)999), // Invalid VehicleType
            new("Brand", "Model", EngineType.Fuel, 2020, VehicleType.Car, new string('A', 16)), // VIN too short
            new("Brand", "Model", EngineType.Fuel, 2020, VehicleType.Car, new string('A', 18)), // VIN too long
        };

        foreach (var command in invalidUpdateCommands)
        {
            var response = await Client.PutAsJsonAsync(string.Format(ApiV1Definition.Vehicles.UpdateById, result.Id), command);
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task VehicleNotFoundFlow_OperationsOnNonExistentVehicle_Returns404()
    {
        await CreateAndAuthenticateUser();
        var nonExistentVehicleId = Guid.NewGuid();

        var getResponse = await Client.GetAsync(string.Format(ApiV1Definition.Vehicles.GetById, nonExistentVehicleId));
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var updateRequest = new UpdateVehicleRequest("Brand", "Model", EngineType.Fuel);
        var updateResponse = await Client.PutAsJsonAsync(string.Format(ApiV1Definition.Vehicles.UpdateById, nonExistentVehicleId), updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var deleteResponse = await Client.DeleteAsync(string.Format(ApiV1Definition.Vehicles.DeleteById, nonExistentVehicleId));
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}