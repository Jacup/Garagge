using Application.Abstractions.Authentication;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Domain.Enums.Services;
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
                EngineType = EngineType.Hybrid,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "BMW",
                Model = "X5",
                ManufacturedYear = 2021,
                UserId = users[0].Id,
                EngineType = EngineType.Fuel,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Tesla",
                Model = "Model 3",
                ManufacturedYear = 2019,
                UserId = users[0].Id,
                EngineType = EngineType.Electric,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Audi",
                Model = "A4",
                ManufacturedYear = 2022,
                UserId = users[1].Id,
                EngineType = EngineType.Fuel,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Brand = "Mercedes-Benz",
                Model = "C-Class",
                ManufacturedYear = 2021,
                UserId = users[1].Id,
                EngineType = EngineType.PlugInHybrid,
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

        var serviceTypes = new List<ServiceType>
        {
            new() { Id = Guid.NewGuid(), Name = "General" },
            new() { Id = Guid.NewGuid(), Name = "OilChange" },
            new() { Id = Guid.NewGuid(), Name = "Brakes" },
            new() { Id = Guid.NewGuid(), Name = "Tires" },
            new() { Id = Guid.NewGuid(), Name = "Engine" },
            new() { Id = Guid.NewGuid(), Name = "Transmission" },
            new() { Id = Guid.NewGuid(), Name = "Suspension" },
            new() { Id = Guid.NewGuid(), Name = "Electrical" },
            new() { Id = Guid.NewGuid(), Name = "Bodywork" },
            new() { Id = Guid.NewGuid(), Name = "Interior" },
            new() { Id = Guid.NewGuid(), Name = "Inspection" },
            new() { Id = Guid.NewGuid(), Name = "Emergency" },
            new() { Id = Guid.NewGuid(), Name = "Other" },
        };

        await context.ServiceTypes.AddRangeAsync(serviceTypes);
        await context.SaveChangesAsync();

        var serviceRecords = new List<ServiceRecord>
        {
            // Toyota Corolla service records
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2022, 1, 15, 0, 0, 0, DateTimeKind.Utc), Mileage = 10000 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[0].Id, Title = "Regular Service", ServiceDate = new DateTime(2022, 2, 10, 0, 0, 0, DateTimeKind.Utc), Mileage = 11000, Notes = "Full inspection", ManualCost = 150.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[2].Id, Title = "Brake Pads Replacement", ServiceDate = new DateTime(2022, 3, 5, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[3].Id, Title = "Tire Rotation", ServiceDate = new DateTime(2022, 4, 12, 0, 0, 0, DateTimeKind.Utc), Mileage = 12500, ManualCost = 50.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2022, 5, 20, 0, 0, 0, DateTimeKind.Utc), Mileage = 13000 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[7].Id, Title = "Battery Check", ServiceDate = new DateTime(2022, 6, 8, 0, 0, 0, DateTimeKind.Utc), Notes = "Battery OK", ManualCost = 0m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[10].Id, Title = "Annual Inspection", ServiceDate = new DateTime(2022, 7, 1, 0, 0, 0, DateTimeKind.Utc), Mileage = 13800, ManualCost = 120.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[3].Id, Title = "New Tires", ServiceDate = new DateTime(2022, 8, 15, 0, 0, 0, DateTimeKind.Utc), Mileage = 14200 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil and Filter Change", ServiceDate = new DateTime(2022, 9, 10, 0, 0, 0, DateTimeKind.Utc), Mileage = 14800, ManualCost = 80.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[6].Id, Title = "Suspension Check", ServiceDate = new DateTime(2022, 10, 5, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[0].Id, Title = "General Maintenance", ServiceDate = new DateTime(2022, 11, 12, 0, 0, 0, DateTimeKind.Utc), Mileage = 15200, Notes = "All good" },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[2].Id, Title = "Brake Fluid Change", ServiceDate = new DateTime(2022, 12, 3, 0, 0, 0, DateTimeKind.Utc), Mileage = 15600, ManualCost = 65.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2023, 1, 8, 0, 0, 0, DateTimeKind.Utc), Mileage = 16000 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[7].Id, Title = "Headlight Replacement", ServiceDate = new DateTime(2023, 2, 14, 0, 0, 0, DateTimeKind.Utc), ManualCost = 45.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[9].Id, Title = "Interior Cleaning", ServiceDate = new DateTime(2023, 3, 7, 0, 0, 0, DateTimeKind.Utc), Mileage = 16800 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[4].Id, Title = "Engine Diagnostics", ServiceDate = new DateTime(2023, 4, 11, 0, 0, 0, DateTimeKind.Utc), Mileage = 17200, Notes = "No issues found", ManualCost = 90.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2023, 5, 15, 0, 0, 0, DateTimeKind.Utc), Mileage = 17800 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[3].Id, Title = "Tire Pressure Check", ServiceDate = new DateTime(2023, 6, 2, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[8].Id, Title = "Paint Touch Up", ServiceDate = new DateTime(2023, 7, 9, 0, 0, 0, DateTimeKind.Utc), ManualCost = 200.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[10].Id, Title = "Safety Inspection", ServiceDate = new DateTime(2023, 8, 5, 0, 0, 0, DateTimeKind.Utc), Mileage = 18500, ManualCost = 100.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2023, 9, 12, 0, 0, 0, DateTimeKind.Utc), Mileage = 19000 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[6].Id, Title = "Shock Absorbers", ServiceDate = new DateTime(2023, 10, 8, 0, 0, 0, DateTimeKind.Utc), Notes = "Replaced rear shocks" },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[0].Id, Title = "General Service", ServiceDate = new DateTime(2023, 11, 3, 0, 0, 0, DateTimeKind.Utc), Mileage = 19600, ManualCost = 175.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[11].Id, Title = "Emergency Repair", ServiceDate = new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Utc), Mileage = 20000 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc), Mileage = 20500 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[2].Id, Title = "Brake Inspection", ServiceDate = new DateTime(2024, 2, 5, 0, 0, 0, DateTimeKind.Utc), ManualCost = 30.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[5].Id, Title = "Transmission Fluid", ServiceDate = new DateTime(2024, 3, 12, 0, 0, 0, DateTimeKind.Utc), Mileage = 21200 },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[12].Id, Title = "Misc Service", ServiceDate = new DateTime(2024, 4, 8, 0, 0, 0, DateTimeKind.Utc), Notes = "Various small fixes" },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[1].Id, Title = "Oil Change", ServiceDate = new DateTime(2024, 5, 15, 0, 0, 0, DateTimeKind.Utc), Mileage = 22000, ManualCost = 75.00m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicles[0].Id, TypeId = serviceTypes[0].Id, Title = "Full Service", ServiceDate = new DateTime(2024, 6, 10, 0, 0, 0, DateTimeKind.Utc), Mileage = 22500 },
        };

        await context.ServiceRecords.AddRangeAsync(serviceRecords);
        await context.SaveChangesAsync();

        var serviceItems = new List<ServiceItem>
        {
            new() { Id = Guid.NewGuid(), ServiceRecordId = serviceRecords[1].Id, Name = "Oil Filter", Type = ServiceItemType.Part, UnitPrice = 15.00m, Quantity = 1, PartNumber = "OF-123" },
            new() { Id = Guid.NewGuid(), ServiceRecordId = serviceRecords[1].Id, Name = "Labor", Type = ServiceItemType.Labor, UnitPrice = 50.00m, Quantity = 2 },
            new() { Id = Guid.NewGuid(), ServiceRecordId = serviceRecords[7].Id, Name = "Tire Set", Type = ServiceItemType.Part, UnitPrice = 120.00m, Quantity = 4, PartNumber = "TR-456" },
            new() { Id = Guid.NewGuid(), ServiceRecordId = serviceRecords[7].Id, Name = "Installation", Type = ServiceItemType.Labor, UnitPrice = 25.00m, Quantity = 4 },
            new() { Id = Guid.NewGuid(), ServiceRecordId = serviceRecords[15].Id, Name = "Diagnostic", Type = ServiceItemType.Labor, UnitPrice = 90.00m, Quantity = 1, Notes = "Computer scan" },
        };

        await context.ServiceItems.AddRangeAsync(serviceItems);
        await context.SaveChangesAsync();
    }
}