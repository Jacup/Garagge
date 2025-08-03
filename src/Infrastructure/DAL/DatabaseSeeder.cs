using Application.Abstractions.Authentication;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DAL;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        if (await context.Users.AnyAsync() || await context.Vehicles.AnyAsync())
            return;

        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "admin@garagge.app",
                FirstName = "Jan",
                LastName = "Kowalski",
                PasswordHash = passwordHasher.Hash("password123")
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user@garagge.app",
                FirstName = "Anna",
                LastName = "Nowak",
                PasswordHash = passwordHasher.Hash("password123")
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var vehicles = new List<Vehicle>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Toyota",
                Model = "Corolla",
                ManufacturedYear = 2020,
                UserId = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "BMW",
                Model = "X5",
                ManufacturedYear = 2021,
                UserId = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Volkswagen",
                Model = "Golf",
                ManufacturedYear = 2019,
                UserId = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Audi",
                Model = "A4",
                ManufacturedYear = 2022,
                UserId = users[1].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Mercedes-Benz",
                Model = "C-Class",
                ManufacturedYear = 2021,
                UserId = users[1].Id
            }
        };

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();
    }
}
