using ApiIntegrationTests.Definitions;
using Application.Abstractions.Authentication;
using Application.Auth.Login;
using Application.Auth.Register;
using Domain.Entities.Users;
using Infrastructure.DAL;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Fixtures;

public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private IServiceScope _scope = null!;

    protected ApplicationDbContext DbContext = null!;

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
    
    protected async Task RegisterAndAuthenticateUser(string userEmail, string firstName, string lastName, string userPassword)
    {
        await RegisterUser(userEmail, firstName, lastName, userPassword);

        LoginUserResponse loginResult = await LoginUser(userEmail, userPassword);

        Authenticate(loginResult.AccessToken);
    }
    
    protected async Task RegisterUser(string userEmail, string firstName, string lastName, string userPassword)
    {
        var registerRequest = new RegisterUserCommand(userEmail, firstName, lastName, userPassword);
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, registerRequest);

        registerResponse.EnsureSuccessStatusCode();
    }

    protected async Task<LoginUserResponse> LoginUser(string userEmail, string userPassword)
    {
        var loginRequest = new LoginUserCommand(userEmail, userPassword);
        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        loginResult.ShouldNotBeNull();
        loginResult.AccessToken.ShouldNotBeNull();
        return loginResult;
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