using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Application.EnergyEntries;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Vehicles.EnergyEntries;

public class CreateEnergyEntryEndpointTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateEnergyEntry_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user, energyTypes: [EnergyType.Gasoline]);

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: 15000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.5m,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var energyEntry = await response.Content.ReadFromJsonAsync<EnergyEntryDto>(DefaultJsonSerializerOptions);
        energyEntry.ShouldNotBeNull();
        energyEntry.VehicleId.ShouldBe(vehicle.Id);
        energyEntry.Date.ShouldBe(request.Date);
        energyEntry.Mileage.ShouldBe(request.Mileage);
        energyEntry.Type.ShouldBe(request.Type);
        energyEntry.EnergyUnit.ShouldBe(request.EnergyUnit);
        energyEntry.Volume.ShouldBe(request.Volume);
        energyEntry.Cost.ShouldBe(request.Cost);
        energyEntry.PricePerUnit.ShouldBe(request.PricePerUnit);
        energyEntry.Id.ShouldNotBe(Guid.Empty);
        energyEntry.CreatedDate.ShouldNotBe(default);
        energyEntry.UpdatedDate.ShouldNotBe(default);

        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldContain($"/vehicles/{vehicle.Id}/energy-entries/{energyEntry.Id}");
    }

    [Fact]
    public async Task CreateEnergyEntry_ShouldReturnNotFound_WhenVehicleDoesNotExist()
    {
        // Arrange
        await CreateAndAuthenticateUser();

        var nonExistentVehicleId = Guid.NewGuid();
        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: 15000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.5m,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, nonExistentVehicleId),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateEnergyEntry_ShouldReturnNotFound_WhenUserDoesNotOwnVehicle()
    {
        // Arrange
        User owner = await CreateUserAsync("owner@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(owner);

        await CreateAndAuthenticateUser();

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: 15000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.5m,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateEnergyEntry_ShouldReturnBadRequest_WhenVolumeIsInvalid(decimal volume)
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: 15000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: volume,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEnergyEntry_ShouldReturnBadRequest_WhenMileageIsNegative()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: -1,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.5m,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEnergyEntry_ShouldReturnBadRequest_WhenDateIsInFuture()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user);

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(1)), // Future date
            Mileage: 15000,
            Type: EnergyType.Gasoline,
            EnergyUnit: EnergyUnit.Liter,
            Volume: 50.5m,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEnergyEntry_ShouldReturnCreated_WhenOptionalFieldsAreNull()
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user, energyTypes: [EnergyType.Electric]);

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: 15000,
            Type: EnergyType.Electric,
            EnergyUnit: EnergyUnit.kWh,
            Volume: 50.5m,
            Cost: null, // Optional
            PricePerUnit: null // Optional
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var energyEntry = await response.Content.ReadFromJsonAsync<EnergyEntryDto>(DefaultJsonSerializerOptions);
        energyEntry.ShouldNotBeNull();
        energyEntry.Cost.ShouldBeNull();
        energyEntry.PricePerUnit.ShouldBeNull();
    }

    [Theory]
    [InlineData(EnergyType.Gasoline, EnergyUnit.Liter)]
    [InlineData(EnergyType.Diesel, EnergyUnit.Liter)]
    [InlineData(EnergyType.Electric, EnergyUnit.kWh)]
    [InlineData(EnergyType.LPG, EnergyUnit.Liter)]
    [InlineData(EnergyType.CNG, EnergyUnit.CubicMeter)]
    public async Task CreateEnergyEntry_ShouldReturnCreated_ForDifferentEnergyTypes(EnergyType energyType, EnergyUnit energyUnit)
    {
        // Arrange
        await CreateAndAuthenticateUser();
        User user = DbContext.Users.First(u => u.Email == "test@garagge.app");
        Vehicle vehicle = await CreateVehicleAsync(user, energyTypes: [energyType]);

        var request = new CreateEnergyEntryRequest(
            Date: DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            Mileage: 15000,
            Type: energyType,
            EnergyUnit: energyUnit,
            Volume: 50.5m,
            Cost: 250.50m,
            PricePerUnit: 4.98m
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            string.Format(ApiV1Definitions.EnergyEntries.Create, vehicle.Id),
            request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        
        var energyEntry = await response.Content.ReadFromJsonAsync<EnergyEntryDto>(DefaultJsonSerializerOptions);
        energyEntry.ShouldNotBeNull();
        energyEntry.Type.ShouldBe(energyType);
        energyEntry.EnergyUnit.ShouldBe(energyUnit);
    }
}
