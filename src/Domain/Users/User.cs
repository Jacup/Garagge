using Domain.Vehicles;
using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PasswordHash { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
