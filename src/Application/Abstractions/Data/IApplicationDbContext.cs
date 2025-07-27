using Domain.Users;
using Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Vehicle> Vehicles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
