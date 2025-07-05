using Microsoft.Extensions.Configuration;

namespace Infrastructure.DAL;

internal static class DatabaseConfiguration
{
    internal static string GetConnectionString(this IConfiguration configuration)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            var cs = configuration.GetConnectionString("Database");

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string 'Database' is not configured.");

            return cs;
        }
            
        var host = configuration["DB_HOST"] ?? "db";
        var port = configuration["DB_PORT"] ?? "5432";
        var db = configuration["DB_NAME"] ?? "garagge-db";
        var user = configuration["DB_USERNAME"] ?? "postgres";
        var password = configuration["DB_PASSWORD"] ?? "postgres";

        return $"Host={host};Port={port};Database={db};Username={user};Password={password}";
    }
}
