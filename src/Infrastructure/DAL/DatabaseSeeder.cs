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
                UserId = users[0].Id,
                PowerType = PowerType.Hybrid,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "BMW",
                Model = "X5",
                ManufacturedYear = 2021,
                UserId = users[0].Id,
                PowerType = PowerType.Gasoline,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Volkswagen",
                Model = "e-Golf",
                ManufacturedYear = 2019,
                UserId = users[0].Id,
                PowerType = PowerType.Electric,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Audi",
                Model = "A4",
                ManufacturedYear = 2022,
                UserId = users[1].Id,
                PowerType = PowerType.Gasoline,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Mercedes-Benz",
                Model = "C-Class",
                ManufacturedYear = 2021,
                UserId = users[1].Id,
                PowerType = PowerType.PlugInHybrid,
            }
        };

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();

        var energyEntries = new List<EnergyEntry>
        {
            // Fuel Entries for the first vehicle (gasoline - fuel entries only)
            new FuelEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[0].Id,
                Date = new DateOnly(2023, 1, 15),
                Mileage = 15000,
                Cost = 50.75m,
                Volume = 25.5m,
                Unit = VolumeUnit.Liters,
                PricePerUnit = 5.50m
            },
            new FuelEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[0].Id,
                Date = new DateOnly(2023, 1, 20),
                Mileage = 15000,
                Cost = 50.75m,
                Volume = 25.5m,
                Unit = VolumeUnit.Liters,
                PricePerUnit = 5.50m
            },

            // Charging Entries for the second vehicle (hybrid - charging entries onlyy)
            new ChargingEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[2].Id,
                Date = new DateOnly(2023, 2, 20),
                Mileage = 30000,
                Cost = 80.00m,
                EnergyAmount = 100.0m,
                Unit = EnergyUnit.kWh,
                PricePerUnit = 0.80m
            },
            new ChargingEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[2].Id,
                Date = new DateOnly(2023, 2, 25),
                Mileage = 35000,
                Cost = 85.00m,
                EnergyAmount = 80.0m,
                Unit = EnergyUnit.kWh,
                PricePerUnit = 0.70m
            },

            // Mixed Entries for the third vehicle (plugin-hybrid - both entries)
            new ChargingEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[4].Id,
                Date = new DateOnly(2023, 2, 25),
                Mileage = 35000,
                Cost = 250.00m,
                EnergyAmount = 80.0m,
                Unit = EnergyUnit.kWh,
                PricePerUnit = 0.60m
            },
            new FuelEntry
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicles[4].Id,
                Date = new DateOnly(2023, 2, 25),
                Mileage = 35000,
                Cost = 60.50m,
                Volume = 50m,
                Unit = VolumeUnit.Liters,
                PricePerUnit = 4.50m
            },
        };

        IEnumerable<FuelEntry> fuelEntries = energyEntries.Where(e => e is FuelEntry).Cast<FuelEntry>();
        await context.FuelEntries.AddRangeAsync(fuelEntries);

        IEnumerable<ChargingEntry> chargingEntries = energyEntries.Where(e => e is ChargingEntry).Cast<ChargingEntry>();
        await context.ChargingEntries.AddRangeAsync(chargingEntries);

        await context.SaveChangesAsync();
    }
}