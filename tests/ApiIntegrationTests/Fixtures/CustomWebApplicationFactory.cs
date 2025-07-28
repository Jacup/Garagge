using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ApiIntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _environment;
    private readonly Dictionary<string, string?>? _overrides;


    public CustomWebApplicationFactory(string environment = "Development", Dictionary<string, string?>? overrides = null)
    {
        _environment = environment;
        _overrides = overrides;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment(_environment);

        builder.ConfigureAppConfiguration((context, config) =>
        {
            if (_overrides != null)
            {
                config.AddInMemoryCollection(_overrides);
            }
        });

        return base.CreateHost(builder);
    }
}
