using Domain.Entities.EnergyEntries;
using Domain.Entities.Services;
using Domain.Entities.Users;
using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleEnergyType> VehicleEnergyTypes { get; }
    DbSet<EnergyEntry> EnergyEntries { get; }
    
    DbSet<ServiceRecord> ServiceRecords { get; }
    DbSet<ServiceType> ServiceTypes { get; }
    DbSet<ServiceItem> ServiceItems { get; }
    
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
