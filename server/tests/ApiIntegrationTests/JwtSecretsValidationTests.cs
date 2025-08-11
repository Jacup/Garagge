using ApiIntegrationTests.Fixtures;

namespace ApiIntegrationTests;

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
}
