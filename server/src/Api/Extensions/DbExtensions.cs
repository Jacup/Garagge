using Infrastructure.DAL;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions;

public static class DbExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }

    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        
        if (!environment.IsDevelopment())
            return;

        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await DatabaseSeeder.SeedAsync(dbContext, scope.ServiceProvider);
    }
}
