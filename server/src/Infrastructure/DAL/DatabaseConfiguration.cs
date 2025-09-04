using Microsoft.Extensions.Configuration;

namespace Infrastructure.DAL;

internal static class DatabaseConfiguration
{
    internal static string GetConnectionString(this IConfiguration configuration)
    {
        // Try connection string first (Development or explicit connection string)
        var connectionString = configuration.GetConnectionString("Database");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            // Always check for host override (supports Development in Docker containers)
            var dbHost = configuration["DB_HOST"] ?? Environment.GetEnvironmentVariable("DB_HOST");
            if (!string.IsNullOrWhiteSpace(dbHost))
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
                {
                    Host = dbHost
                };
                return builder.ToString();
            }
            return connectionString;
        }
        
        // Build from individual settings (Production or fallback)
        var options = new DatabaseOptions
        {
            Host = configuration["DB_HOST"] ?? Environment.GetEnvironmentVariable("DB_HOST") ?? "db",
            Port = int.TryParse(configuration["DB_PORT"] ?? Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 5432,
            Name = configuration["DB_NAME"] ?? Environment.GetEnvironmentVariable("DB_NAME") ?? "garagge-db", 
            Username = configuration["DB_USERNAME"] ?? Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres",
            Password = configuration["DB_PASSWORD"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD") ?? 
                       throw new InvalidOperationException("Database password (DB_PASSWORD) must be configured")
        };
        
        return options.BuildConnectionString();
    }
}
