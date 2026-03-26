using ApiIntegrationTests.Contracts;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiIntegrationTests.Contracts.V1;
using Application.Abstractions.Authentication;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Domain.Enums.Services;
using Infrastructure.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ApiIntegrationTests.Fixtures;

public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private IServiceScope _scope = null!;
    protected ApplicationDbContext DbContext = null!;
    protected static readonly JsonSerializerOptions DefaultJsonSerializerOptions;

    static BaseIntegrationTest()
    {
        DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        DefaultJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();
    }

    protected HttpClient Client { get; init; }

    protected CustomWebApplicationFactory Factory { get; }

    protected async Task<User> CreateUserAsync(
        string email = "test@garagge.app",
        string password = "Password123",
        string firstName = "John",
        string lastName = "Doe")
    {
        var passwordHasher = Factory.Services.GetRequiredService<IPasswordHasher>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHasher.Hash(password),
        };

        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        return user;
    }

    protected async Task CreateAndAuthenticateUser(bool rememberMe = false)
    {
        await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123", rememberMe);
    }

    protected async Task RegisterAndAuthenticateUser(string userEmail, string firstName, string lastName, string userPassword, bool rememberMe = false)
    {
        await RegisterUser(userEmail, firstName, lastName, userPassword);
        await LoginUser(userEmail, userPassword, rememberMe);
    }

    protected async Task RegisterUser(string userEmail, string firstName, string lastName, string userPassword)
    {
        var registerRequest = new RegisterRequest(userEmail, userPassword, firstName, lastName);
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, registerRequest);

        registerResponse.EnsureSuccessStatusCode();
    }

    protected async Task LoginUser(string userEmail, string userPassword, bool rememberMe = false)
    {
        var loginRequest = new LoginRequest(userEmail, userPassword, rememberMe);
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<Vehicle> CreateVehicleAsync(
        User owner,
        string brand = "Toyota",
        string model = "Corolla",
        EngineType engineType = EngineType.Fuel,
        int? manufacturedYear = 2020,
        VehicleType? type = VehicleType.Car,
        string? vin = null,
        IEnumerable<EnergyType>? energyTypes = null)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            Brand = brand,
            Model = model,
            EngineType = engineType,
            ManufacturedYear = manufacturedYear,
            Type = type,
            VIN = vin
        };

        DbContext.Vehicles.Add(vehicle);
        await DbContext.SaveChangesAsync();

        // Add energy types if provided
        var energyTypesToAdd = energyTypes ?? [EnergyType.Gasoline];

        foreach (var energyType in energyTypesToAdd)
        {
            var vehicleEnergyType = new VehicleEnergyType
            {
                Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = energyType
            };

            DbContext.VehicleEnergyTypes.Add(vehicleEnergyType);
        }

        await DbContext.SaveChangesAsync();

        return vehicle;
    }

    protected async Task<Vehicle> CreateVehicleAsync(string brand = "Toyota", string model = "Corolla")
    {
        var user = DbContext.Users.FirstOrDefault(u => u.Email == "test@garagge.app");

        if (user == null)
        {
            throw new InvalidOperationException("No authenticated user found. Call CreateAndAuthenticateUser() first.");
        }

        return await CreateVehicleAsync(user, brand, model);
    }

    protected async Task<ServiceRecord> CreateServiceRecordAsync(
        Guid vehicleId,
        ServiceRecordType type = ServiceRecordType.Other,
        string title = "Oil Change",
        DateTime? serviceDate = null,
        int? mileage = null,
        decimal? cost = null,
        string? notes = null)
    {
        var serviceRecord = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Type = type,
            Title = title,
            ServiceDate = serviceDate.HasValue ? DateTime.SpecifyKind(serviceDate.Value, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(-1),
            Mileage = mileage,
            ManualCost = cost,
            Notes = notes
        };

        DbContext.ServiceRecords.Add(serviceRecord);
        await DbContext.SaveChangesAsync();

        return serviceRecord;
    }

    protected async Task CreateServiceItemAsync(
        Guid serviceRecordId,
        string name = "Service Item",
        decimal unitPrice = 100m,
        decimal quantity = 1,
        string? partNumber = null,
        string? notes = null)
    {
        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecordId,
            Name = name,
            Type = ServiceItemType.Part,
            UnitPrice = unitPrice,
            Quantity = quantity,
            PartNumber = partNumber,
            Notes = notes
        };

        DbContext.ServiceItems.Add(serviceItem);
        await DbContext.SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
        RefreshServices();
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        DbContext.Dispose();

        return Task.CompletedTask;
    }

    private void RefreshServices()
    {
        _scope.Dispose();
        _scope = Factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}