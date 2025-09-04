using Api.Extensions;
using Api.Infrastructure;
using System.Reflection;

namespace Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
    
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpoints(Assembly.GetExecutingAssembly());

        if (EnvironmentExtensions.IsDevelopment())
            return services.AddCorsConfiguration(configuration);

        return services;
    }
}