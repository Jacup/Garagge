using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Infrastructure.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ValidateJwtSecret_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        IConfiguration? config = null;
        var envMock = new Mock<IHostEnvironment>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config!.ValidateJwtSecret(envMock.Object));
    }

    [Fact]
    public void ValidateJwtSecret_NullEnvironment_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        IHostEnvironment? env = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.ValidateJwtSecret(env!));
    }

    [Fact]
    public void ValidateJwtSecret_SecretPresent_DoesNotThrow()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("Jwt:Secret", "abc123")
            ])
            .Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act & Assert
        config.ValidateJwtSecret(envMock.Object);
    }

    [Fact]
    public void ValidateJwtSecret_MissingSecretOnDev_DoesNotThrow()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Act & Assert
        config.ValidateJwtSecret(envMock.Object);
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void ValidateJwtSecret_MissingSecretOnNonProd_ThrowsInvalidOperationException(string env)
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(env);

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() => config.ValidateJwtSecret(envMock.Object));

        ex.Message.ShouldContain("JWT Secret is not configured");
    }
}
