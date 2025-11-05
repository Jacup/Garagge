using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.Core;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Vehicles.ServiceRecords;

public class GetServiceRecordsEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    #region Authentication & Authorization Tests

    [Fact]
    public async Task GetServiceRecords_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicleId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnUnauthorized_WhenVehicleDoesNotBelongToUser()
    {
        // Arrange
        User owner = await CreateUserAsync("owner@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(owner);
        ServiceType serviceType = await CreateServiceTypeAsync();
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        await CreateAndAuthenticateUser();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnNotFound_WhenVehicleDoesNotExist()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        var nonExistentVehicleId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(nonExistentVehicleId));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnOk_WhenUserOwnsVehicle()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Empty Results Tests

    [Fact]
    public async Task GetServiceRecords_ShouldReturnEmptyPagedList_WhenVehicleHasNoServiceRecords()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnEmptyPagedList_WhenFiltersMatchNoRecords()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?searchTerm=NonExistent");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    #endregion

    #region Basic Functionality Tests

    [Fact]
    public async Task GetServiceRecords_ShouldReturnPagedList_WhenVehicleHasServiceRecords()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
       
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Oil Change", DateTime.UtcNow.AddDays(-10), 1000, 150m);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Tire Rotation", DateTime.UtcNow.AddDays(-5), 1200, 75m);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Brake Inspection", DateTime.UtcNow.AddDays(-2), 1500, 200m);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnCorrectDtoStructure_WithAllProperties()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync("Oil Change");
        var serviceDate = new DateTime(2024, 5, 15, 10, 30, 0);
        
        await CreateServiceRecordAsync(
            vehicle.Id, 
            serviceType.Id, 
            "Premium Oil Change", 
            serviceDate, 
            15000, 
            150.75m, 
            "Used synthetic oil");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(1);
        
        var dto = result.Items[0];
        dto.Id.ShouldNotBe(Guid.Empty);
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
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnDefaultSortByServiceDateDescending()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        var record1 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2024, 1, 1));
        var record2 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 3, 1));
        var record3 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2024, 2, 1));

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.Items[0].Id.ShouldBe(record2.Id); // 2024-03-01 (latest)
        result.Items[1].Id.ShouldBe(record3.Id); // 2024-02-01
        result.Items[2].Id.ShouldBe(record1.Id); // 2024-01-01 (earliest)
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetServiceRecords_ShouldReturnFirstPage_WhenPageIsOne()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        for (int i = 0; i < 5; i++)
        {
            await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, $"Service {i}", DateTime.UtcNow.AddDays(-i));
        }

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?page=1&pageSize=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(5);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(2);
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnSecondPage_WhenPageIsTwo()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        for (int i = 0; i < 5; i++)
        {
            await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, $"Service {i}", DateTime.UtcNow.AddDays(-i));
        }

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?page=2&pageSize=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(5);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(2);
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnCorrectPageMetadata_HasNextAndPreviousPage()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        for (int i = 0; i < 10; i++)
        {
            await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, $"Service {i}", DateTime.UtcNow.AddDays(-i));
        }

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?page=2&pageSize=3");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(10);
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetServiceRecords_ShouldRespectPageSize()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        for (int i = 0; i < 10; i++)
        {
            await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, $"Service {i}", DateTime.UtcNow.AddDays(-i));
        }

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?pageSize=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(5);
        result.PageSize.ShouldBe(5);
    }

    #endregion

    #region Filtering - SearchTerm Tests

    [Fact]
    public async Task GetServiceRecords_ShouldFilterBySearchTerm_InTitle()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Tire Rotation");
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Oil Filter Replacement");

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?searchTerm=Oil");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.Title.Contains("Oil"));
    }

    [Fact]
    public async Task GetServiceRecords_ShouldFilterBySearchTerm_InNotes()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", notes: "Changed synthetic oil");
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", notes: "Regular maintenance");
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", notes: "Used synthetic parts");

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?searchTerm=synthetic");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldFilterBySearchTerm_CaseInsensitive()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Tire Rotation");

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?searchTerm=oil");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(1);
    }

    #endregion

    #region Filtering - ServiceTypeId Tests

    [Fact]
    public async Task GetServiceRecords_ShouldFilterByServiceTypeId()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType oilChangeType = await CreateServiceTypeAsync("Oil Change");
        ServiceType tireServiceType = await CreateServiceTypeAsync("Tire Service");
        
        await CreateServiceRecordAsync(vehicle.Id, oilChangeType.Id, "Oil Change 1");
        await CreateServiceRecordAsync(vehicle.Id, tireServiceType.Id, "Tire Rotation");
        await CreateServiceRecordAsync(vehicle.Id, oilChangeType.Id, "Oil Change 2");

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?serviceTypeId={oilChangeType.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.TypeId == oilChangeType.Id);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnEmpty_WhenServiceTypeIdDoesNotMatch()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id);
        
        var nonExistentTypeId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?serviceTypeId={nonExistentTypeId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
    }

    #endregion

    #region Filtering - Date Range Tests

    [Fact]
    public async Task GetServiceRecords_ShouldFilterByDateFrom()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2024, 1, 1));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 5, 15));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2024, 8, 1));

        var dateFrom = new DateTime(2024, 5, 1);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?dateFrom={dateFrom:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.ServiceDate >= dateFrom);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldFilterByDateTo()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2024, 1, 1));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 5, 15));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2024, 8, 1));

        var dateTo = new DateTime(2024, 5, 31);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.ServiceDate <= dateTo);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldFilterByDateRange_BothDates()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2024, 1, 1));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 5, 15));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2024, 6, 1));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 4", new DateTime(2024, 8, 1));

        var dateFrom = new DateTime(2024, 5, 1);
        var dateTo = new DateTime(2024, 6, 30);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.ServiceDate >= dateFrom && x.ServiceDate <= dateTo);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldIncludeRecordsOnBoundaryDates()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        var boundaryDate1 = new DateTime(2024, 5, 1);
        var boundaryDate2 = new DateTime(2024, 5, 31);
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", boundaryDate1);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 5, 15));
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", boundaryDate2);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?dateFrom={boundaryDate1:yyyy-MM-dd}&dateTo={boundaryDate2:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
    }

    #endregion

    #region Filtering - Combined Tests

    [Fact]
    public async Task GetServiceRecords_ShouldApplyMultipleFilters_Correctly()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType oilChangeType = await CreateServiceTypeAsync("Oil Change");
        ServiceType tireServiceType = await CreateServiceTypeAsync("Tire Service");
        
        await CreateServiceRecordAsync(vehicle.Id, oilChangeType.Id, "Premium Oil Change", new DateTime(2024, 5, 15), notes: "Premium service");
        await CreateServiceRecordAsync(vehicle.Id, oilChangeType.Id, "Standard Oil Change", new DateTime(2024, 1, 15), notes: "Standard");
        await CreateServiceRecordAsync(vehicle.Id, tireServiceType.Id, "Premium Tire Service", new DateTime(2024, 5, 20), notes: "Premium tires");

        var dateFrom = new DateTime(2024, 5, 1);
        var dateTo = new DateTime(2024, 5, 31);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?searchTerm=Premium&serviceTypeId={oilChangeType.Id}&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(1);
        result.Items[0].Title.ShouldBe("Premium Oil Change");
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnEmpty_WhenMultipleFiltersMatchNothing()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Oil Change", new DateTime(2024, 5, 15));

        var dateFrom = new DateTime(2024, 6, 1);
        var dateTo = new DateTime(2024, 6, 30);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?searchTerm=Tire&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetServiceRecords_ShouldSortByServiceDate_Ascending()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        var record1 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2024, 3, 1));
        var record2 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 1, 1));
        var record3 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2024, 2, 1));

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?sortBy=servicedate&sortDescending=false");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items[0].Id.ShouldBe(record2.Id); // 2024-01-01
        result.Items[1].Id.ShouldBe(record3.Id); // 2024-02-01
        result.Items[2].Id.ShouldBe(record1.Id); // 2024-03-01
    }

    [Fact]
    public async Task GetServiceRecords_ShouldSortByServiceDate_Descending()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        var record1 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", new DateTime(2024, 1, 1));
        var record2 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", new DateTime(2024, 3, 1));
        var record3 = await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", new DateTime(2024, 2, 1));

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?sortBy=servicedate&sortDescending=true");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items[0].Id.ShouldBe(record2.Id); // 2024-03-01
        result.Items[1].Id.ShouldBe(record3.Id); // 2024-02-01
        result.Items[2].Id.ShouldBe(record1.Id); // 2024-01-01
    }

    [Fact]
    public async Task GetServiceRecords_ShouldSortByMileage_Descending()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1", mileage: 1000);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 2", mileage: 3000);
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 3", mileage: 2000);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?sortBy=mileage&sortDescending=true");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items[0].Mileage.ShouldBe(3000);
        result.Items[1].Mileage.ShouldBe(2000);
        result.Items[2].Mileage.ShouldBe(1000);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldSortByTitle_Ascending()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "C Service");
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "A Service");
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "B Service");

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?sortBy=title&sortDescending=false");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items[0].Title.ShouldBe("A Service");
        result.Items[1].Title.ShouldBe("B Service");
        result.Items[2].Title.ShouldBe("C Service");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GetServiceRecords_ShouldReturnBadRequest_WhenPageIsZero()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?page=0");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnBadRequest_WhenPageIsNegative()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?page=-1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnBadRequest_WhenPageSizeIsZero()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?pageSize=0");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnBadRequest_WhenPageSizeExceedsMaximum()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?pageSize=101");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldReturnBadRequest_WhenDateFromIsAfterDateTo()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        var dateFrom = new DateTime(2024, 6, 1);
        var dateTo = new DateTime(2024, 5, 1);

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GetServiceRecords_ShouldHandleNullOptionalParameters()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, "Service 1");

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Services.GetAll.WithId(vehicle.Id));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetServiceRecords_ShouldWorkWithMaximumPageSize()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);
        ServiceType serviceType = await CreateServiceTypeAsync();
        
        for (int i = 0; i < 50; i++)
        {
            await CreateServiceRecordAsync(vehicle.Id, serviceType.Id, $"Service {i}");
        }

        // Act
        var response = await Client.GetAsync($"{ApiV1Definition.Services.GetAll.WithId(vehicle.Id)}?pageSize=100");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedList<ServiceRecordDto>>(DefaultJsonSerializerOptions);
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(50);
        result.PageSize.ShouldBe(100);
    }

    #endregion
}

