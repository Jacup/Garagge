using Domain.Enums;

namespace Domain.Entities.Vehicles;
public sealed class VehicleEnergyType : Entity
{
    public required Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public required EnergyType EnergyType { get; set; }
}