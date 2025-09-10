namespace ApiIntegrationTests.Fixtures;

public class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    public BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Client = factory.CreateClient();
    }
    
    protected HttpClient Client { get; init; }
}