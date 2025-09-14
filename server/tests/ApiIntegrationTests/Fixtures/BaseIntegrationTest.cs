using Application.Abstractions.Authentication;
using Domain.Entities.Users;
using Infrastructure.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Security.Claims;

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