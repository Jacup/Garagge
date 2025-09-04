using Domain.Entities.EnergyEntries;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Vehicle> Vehicles { get; }
    DbSet<EnergyEntry> EnergyEntries { get; }
    DbSet<FuelEntry> FuelEntries { get; }
    DbSet<ChargingEntry> ChargingEntries { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
