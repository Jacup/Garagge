using Api.Integration.Tests.Fixtures;

namespace Api.Integration.Tests;

public class JwtSecretsValidationTests
{
    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void CreateClient_MissingJwtSecretInStaging_ThrowsException(string env)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory(env);

        // Act & Assert
        var ex = Should.Throw<Exception>(() =>
        {
            using var client = factory.CreateClient();
        });

        ex.Message.ShouldContain("JWT Secret is not configured");
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void CreateClient_WithJwtSecretInStaging_DoesNotThrowException(string env)
    {
        // Arrange
        var overrides = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test-secret"
        };

        using var factory = new CustomWebApplicationFactory(env, overrides);

        // Act & Assert
        using var client = factory.CreateClient();

        client.ShouldNotBeNull();
    }

    [Fact]
    public void CreateClient_MissingJwtSecretInDevelopment_DoesNotThrowException()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory("Development");

        // Act & Assert
        using var client = factory.CreateClient();

        client.ShouldNotBeNull();
    }
}
