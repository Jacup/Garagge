using Application.Abstractions.Authentication;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Domain.Enums;
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
                Email = "john.doe@garagge.app",
                FirstName = "John",
                LastName = "Doe",
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
                UserId = users[0].Id,
                PowerType = EngineType.Hybrid,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "BMW",
                Model = "X5",
                ManufacturedYear = 2021,
                UserId = users[0].Id,
                PowerType = EngineType.Fuel,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Tesla",
                Model = "Model 3",
                ManufacturedYear = 2019,
                UserId = users[0].Id,
                PowerType = EngineType.Electric,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Audi",
                Model = "A4",
                ManufacturedYear = 2022,
                UserId = users[1].Id,
                PowerType = EngineType.Fuel,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Mercedes-Benz",
                Model = "C-Class",
                ManufacturedYear = 2021,
                UserId = users[1].Id,
                PowerType = EngineType.PlugInHybrid,
            }
        };

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();

        var vets = new List<VehicleEnergyType>
        {
            new()
            {
                VehicleId = vehicles[0].Id, // corolla - hybrid
                EnergyType = EnergyType.Gasoline
            },
            new()
            {
                VehicleId = vehicles[2].Id, // Tesla 3 - electric
                EnergyType = EnergyType.Electric
            },
            new()
            {
                VehicleId = vehicles[3].Id, // Audi - gasoline + lpg
                EnergyType = EnergyType.Gasoline
            },
            new() { VehicleId = vehicles[3].Id, EnergyType = EnergyType.LPG },
            new()
            {
                VehicleId = vehicles[4].Id, // Mercedes - plugin-hybrid
                EnergyType = EnergyType.Gasoline
            },
            new() { VehicleId = vehicles[4].Id, EnergyType = EnergyType.Electric }
        };

        await context.VehicleEnergyTypes.AddRangeAsync(vets);
        await context.SaveChangesAsync();
        
        var energyEntries = new List<EnergyEntry>
        {
            // first vehicle (gasoline entries only)
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[0].Id,
                Date = new DateOnly(2023, 1, 15),
                Mileage = 15000,
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 25.5m,
                Cost = 50.75m,
                PricePerUnit = 5.50m
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[0].Id,
                Date = new DateOnly(2023, 1, 20),
                Mileage = 15500,
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 25.0m,
            },

            // Second vehicle (tesla - charging entries only)
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[2].Id,
                Date = new DateOnly(2023, 1, 18),
                Mileage = 10000,
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Volume = 80.0m,
                Cost = 150.00m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[2].Id,
                Date = new DateOnly(2023, 1, 21),
                Mileage = 10500,
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Volume = 80.0m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[2].Id,
                Date = new DateOnly(2023, 1, 25),
                Mileage = 11000,
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Volume = 80.0m,
            },

            // third vehicle (plugin-hybrid - gasoline and electric entries)
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[4].Id,
                Date = new DateOnly(2023, 1, 25),
                Mileage = 11000,
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Volume = 80.0m,
            },
            new()
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[4].Id,
                Date = new DateOnly(2023, 1, 25),
                Mileage = 11000,
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Volume = 30.0m,
                Cost = 250.00m,
            },
        };

        await context.EnergyEntries.AddRangeAsync(energyEntries);
        await context.SaveChangesAsync();
    }
}