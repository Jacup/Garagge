using Api.Infrastructure;

namespace Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(corsSettings.AllowedOrigins)
                    .WithMethods(corsSettings.AllowedMethods)
                    .WithHeaders(corsSettings.AllowedHeaders);
                
                if (corsSettings.AllowedOrigins.Length != 0)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        return app.UseCors();
    }
}