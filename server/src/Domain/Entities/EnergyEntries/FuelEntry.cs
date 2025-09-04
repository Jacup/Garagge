using Domain.Enums;

namespace Domain.Entities.EnergyEntries;

public sealed class FuelEntry : EnergyEntry
{
    public required decimal Volume { get; set; }
    public required VolumeUnit Unit { get; set; }
}