using Api.Extensions;
using Api.Infrastructure;

namespace Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();


        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
    
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCorsConfiguration(configuration);
    }
}