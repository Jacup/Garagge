using Api.Integration.Tests.Fixtures;
using System.Net;

namespace Api.Integration.Tests;

public class ApiStartupTests
{
    [Fact]
    public async Task App_Starts_And_HealthEndpoint_Returns_OK()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory("Development");
        var client = factory.CreateClient();
        var uri = new Uri("/health", UriKind.Relative);

        // Act
        var response = await client.GetAsync(uri);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
