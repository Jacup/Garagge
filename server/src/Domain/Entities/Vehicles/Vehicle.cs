using Domain.Entities.EnergyEntries;
using Domain.Entities.Users;
using Domain.Enums;

namespace Domain.Entities.Vehicles;

public sealed class Vehicle : Entity
{
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public required EngineType EngineType { get; set; }

    public int? ManufacturedYear { get; set; }
    public VehicleType? Type { get; set; }
    public string? VIN { get; set; }
    
    public required Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<VehicleEnergyType> VehicleEnergyTypes { get; set; } = [];
    public ICollection<EnergyEntry> EnergyEntries { get; set; } = [];

    public IEnumerable<EnergyType> AllowedEnergyTypes => VehicleEnergyTypes.Select(vet => vet.EnergyType);
}