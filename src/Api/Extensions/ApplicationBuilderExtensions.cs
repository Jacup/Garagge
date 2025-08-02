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
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTheme(ScalarTheme.Kepler)
                .WithDarkModeToggle(true)
                .WithClientButton(true);
        });
        
        if (useSwaggerWithOpenApi)
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1 API"));

        return app;
    }
}