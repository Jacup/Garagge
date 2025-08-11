using Domain.Entities.Vehicles;

namespace Domain.Entities.EnergyEntries;

public abstract class EnergyEntry : Entity
{
    public required DateOnly Date { get; set; }
    public required int Mileage { get; set;  }
    public required decimal Cost { get; set; }
    
    public required Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}