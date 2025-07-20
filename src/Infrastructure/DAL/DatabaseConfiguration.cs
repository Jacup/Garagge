using Microsoft.Extensions.Configuration;

namespace Infrastructure.DAL;

internal static class DatabaseConfiguration
{
    internal static string GetConnectionString(this IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var dbHost = configuration["DB_HOST"] ?? Environment.GetEnvironmentVariable("DB_HOST");

        if (environment == "Development")
        {
            var cs = configuration.GetConnectionString("Database");

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string 'Database' is not configured.");

            if (!string.IsNullOrWhiteSpace(dbHost))
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(cs)
                {
                    Host = dbHost
                };

                return builder.ToString();
            }
            
            return cs;
        }
            
        var host = dbHost ?? "db";
        var portProd = configuration["DB_PORT"] ?? Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
        var dbProd = configuration["DB_NAME"] ?? Environment.GetEnvironmentVariable("DB_NAME") ?? "garagge-db";
        var userProd = configuration["DB_USERNAME"] ?? Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres";
        var passwordProd = configuration["DB_PASSWORD"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres";

        return $"Host={host};Port={portProd};Database={dbProd};Username={userProd};Password={passwordProd}";
    }
}
