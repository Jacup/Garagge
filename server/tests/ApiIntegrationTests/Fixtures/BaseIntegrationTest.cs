using ApiIntegrationTests.Contracts;
using System.Net;
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
using Infrastructure.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace ApiIntegrationTests.Fixtures;

public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IServiceScope _scope = null!;
    protected ApplicationDbContext DbContext = null!;
    protected static readonly JsonSerializerOptions DefaultJsonSerializerOptions;

    static BaseIntegrationTest()
    {
        DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        DefaultJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
    
    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient();
    }

    protected HttpClient Client { get; init; }

    protected async Task<User> CreateUserAsync(
        string email = "test@garagge.app",
        string password = "Password123",
        string firstName = "John",
        string lastName = "Doe")
    {
        var passwordHasher = _factory.Services.GetRequiredService<IPasswordHasher>();
        
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

        var result = await LoginUser("test@garagge.app", "Password123", rememberMe);

        Authenticate(result.AccessToken);
    }
    
    protected async Task RegisterAndAuthenticateUser(string userEmail, string firstName, string lastName, string userPassword, bool rememberMe = false)
    {
        await RegisterUser(userEmail, firstName, lastName, userPassword);

        var result = await LoginUser(userEmail, userPassword, rememberMe);

        Authenticate(result.AccessToken);
    }
    
    protected async Task RegisterUser(string userEmail, string firstName, string lastName, string userPassword)
    {
        var registerRequest = new RegisterRequest(userEmail, userPassword, firstName, lastName);
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, registerRequest);

        registerResponse.EnsureSuccessStatusCode();
    }

    protected async Task<(string AccessToken, string RefreshToken)> LoginUser(string userEmail, string userPassword, bool rememberMe = false)
    {
        var loginRequest = new LoginRequest(userEmail, userPassword, rememberMe);
        var response = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);

        response.EnsureSuccessStatusCode();

        var accessToken = ParseAccessTokenFromCookie(response.Headers);
        accessToken.ShouldNotBeNullOrEmpty();

        var refreshToken = ParseRefreshTokenFromCookie(response.Headers);
        refreshToken.ShouldNotBeNullOrEmpty();
        
        return (accessToken, refreshToken);
    }
    
    protected static string? ParseAccessTokenFromCookie(HttpResponseHeaders headers, bool expired = false)
    {
        var cookieHeader = headers.GetValues("Set-Cookie").FirstOrDefault();
        if (cookieHeader is null) 
            return null;

        if (expired)
            return cookieHeader.StartsWith("accessToken=;") ? "" : null;

        var parts = cookieHeader.Split(';')[0].Split('=');
        if (parts.Length != 2)
            return null;

        var encodedValue = parts[1];
        return WebUtility.UrlDecode(encodedValue);
    }
    
    protected static string? ParseRefreshTokenFromCookie(HttpResponseHeaders headers, bool expired = false)
    {
        var cookieHeader = headers.GetValues("Set-Cookie").FirstOrDefault();
        if (cookieHeader is null) 
            return null;

        if (expired)
            return cookieHeader.StartsWith("refreshToken=;") ? "" : null;

        var parts = cookieHeader.Split(';')[0].Split('=');
        if (parts.Length != 2)
            return null;

        var encodedValue = parts[1];
        return WebUtility.UrlDecode(encodedValue);
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
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                EnergyType = energyType
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

    protected async Task<ServiceType> CreateServiceTypeAsync(string name = "Maintenance")
    {
        var serviceType = new ServiceType
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        DbContext.ServiceTypes.Add(serviceType);
        await DbContext.SaveChangesAsync();

        return serviceType;
    }

    protected async Task<ServiceRecord> CreateServiceRecordAsync(
        Guid vehicleId,
        Guid typeId,
        string title = "Oil Change",
        DateTime? serviceDate = null,
        int? mileage = null,
        decimal? cost = null,
        string? notes = null)
    {
        var serviceType = await DbContext.ServiceTypes.FindAsync(typeId);
        if (serviceType == null)
        {
            throw new InvalidOperationException($"ServiceType with Id {typeId} not found");
        }

        var serviceRecord = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            TypeId = typeId,
            Type = serviceType,
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

    protected async Task<ServiceItem> CreateServiceItemAsync(
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
            Type = Domain.Enums.Services.ServiceItemType.Part,
            UnitPrice = unitPrice,
            Quantity = quantity,
            PartNumber = partNumber,
            Notes = notes
        };

        DbContext.ServiceItems.Add(serviceItem);
        await DbContext.SaveChangesAsync();

        return serviceItem;
    }
    
    protected void Authenticate(string accessToken)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
    
    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        RefreshServices();
    }

    public Task DisposeAsync()
    {
        _scope?.Dispose();
        DbContext?.Dispose();
        return Task.CompletedTask;
    }
    
    private void RefreshServices()
    {
        _scope?.Dispose();
        _scope = _factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
}