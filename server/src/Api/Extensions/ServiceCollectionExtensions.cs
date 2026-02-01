using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "accessToken",
                Description = "Access token cookie",
                In = ParameterLocation.Cookie,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "cookie"
            };

            o.AddSecurityDefinition("cookieAuth", securityScheme);

            o.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, [] } });
        });

        return services;
    }
}