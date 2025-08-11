using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void ValidateJwtSecret(this IConfiguration configuration, IHostEnvironment environment, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        const string fallbackSecret = "default-selfhosted-secret";

        var secret = configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT Secret is not configured. Please set the environment variable 'JWT__SECRET'.");

        if (secret == fallbackSecret)
            logger.LogWarning("You are using the default JWT secret. This is insecure for production. Set a strong secret using 'JWT__SECRET'.");
        else
            logger.LogInformation("Safe secret used.");
    }
}