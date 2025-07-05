using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void ValidateJwtSecret(this IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var secret = configuration["Jwt:Secret"];

        if (string.IsNullOrWhiteSpace(secret) && !environment.IsDevelopment())
            throw new InvalidOperationException("JWT Secret is not configured. Please set Jwt:Secret in your environment for production/staging.");
    }
}
