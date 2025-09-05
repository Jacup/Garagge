using Domain.Entities.Vehicles;
using Domain.Enums;

namespace Domain.Entities.EnergyEntries;

public class EnergyEntry : Entity
{
    public required DateOnly Date { get; set; }
    public required int Mileage { get; set;  }

    public required EnergyType Type { get; set; }
    public required EnergyUnit EnergyUnit { get; set; }
    public required decimal Volume { get; set; }

    public decimal? Cost { get; set; }
    public decimal? PricePerUnit { get; set; }

    public required Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}