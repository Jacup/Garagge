using Domain.Entities.Users;

namespace Domain.Entities.Vehicles;

public sealed class Vehicle : Entity
{
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public required DateOnly ManufacturedYear { get; set; }

    public required Guid UserId { get; set; }
    public User? User { get; set; }
}