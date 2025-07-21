using System.Reflection;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Api;
using Api.Extensions;

Console.WriteLine("App starting...");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddWebApi(builder.Configuration);
builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Configuration.ValidateJwtSecret(app.Environment, logger);

if (EnvironmentExtensions.IsDevelopment())
    app.UseCorsConfiguration();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();


// SPA fallback: return index.html for unknown routes (only in Production)
if (app.Environment.IsProduction())
{
    app.UseStaticFiles();
    app.Use(async (context, next) =>
    {
        await next();
        var path = context.Request.Path.Value ?? string.Empty;
        if (context.Response.StatusCode == 404 && !path.StartsWith("/api"))
        {
            context.Response.StatusCode = 200;
            await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
        }
    });
}

await app.RunAsync();


namespace Api
{
    public partial class Program;
}