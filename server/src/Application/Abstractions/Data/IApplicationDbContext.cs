using Domain.Entities.EnergyEntries;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleEnergyType> VehicleEnergyTypes { get; }
    DbSet<EnergyEntry> EnergyEntries { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
