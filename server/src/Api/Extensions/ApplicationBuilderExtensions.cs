using Scalar.AspNetCore;

namespace Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }

    public static IApplicationBuilder UseOpenApi(this WebApplication app, bool useSwaggerWithOpenApi)
    {
        app.MapOpenApi("/api/openapi/{documentName}.json");
        app.MapScalarApiReference("/api/scalar", options =>
        {
            options
                .WithTheme(ScalarTheme.Kepler)
                .WithDarkModeToggle()
                .WithClientButton()
                .WithOpenApiRoutePattern("/api/openapi/{documentName}.json");
        });
        
        if (useSwaggerWithOpenApi)
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/api/openapi/v1.json", "v1 API"));

        return app;
    }
}