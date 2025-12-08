using Application.Abstractions;
using Application.Abstractions.Services;
using Application.Behaviors;
using Application.Core;
using Application.Services;
using Application.Services.EnergyStats;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IResultFactory, ResultFactory>();
        
        // Register services
        services.AddScoped<IVehicleEngineCompatibilityService, VehicleEngineCompatibilityService>();
        services.AddScoped<IVehicleUpdateValidationService, VehicleUpdateValidationService>();
        services.AddScoped<IEnergyEntryFilterService, EnergyEntryFilterService>();
        services.AddScoped<IEnergyStatsService, EnergyStatsService>();
        services.AddScoped<IEnergyEntryMileageValidator, EnergyEntryMileageValidator>();
        services.AddScoped<IServiceRecordFilterService, ServiceRecordFilterService>();
        
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Configure Mapster for enum mapping
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

        return services;
    }
}
