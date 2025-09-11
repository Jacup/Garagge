using Infrastructure.DAL;
using Microsoft.Extensions.DependencyInjection;

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