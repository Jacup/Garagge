namespace Infrastructure.DAL;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string Host { get; init; } = "db";
    public int Port { get; init; } = 5432;
    public string Name { get; init; } = "garagge-db";
    public string Username { get; init; } = "postgres";
    public string Password { get; init; } = string.Empty;
    public string? ConnectionString { get; init; }
    
    public string BuildConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
            return ConnectionString;
            
        if (string.IsNullOrWhiteSpace(Password))
            throw new InvalidOperationException("Database password must be configured");
            
        return $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
    }
}
