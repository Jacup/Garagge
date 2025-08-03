using Domain.Enums;

namespace Domain.Entities.EnergyEntries;

public sealed class ChargingEntry : EnergyEntry
{
    public required decimal EnergyAmount { get; set; }
    public required EnergyUnit Unit { get; set; } = EnergyUnit.kWh;
    public required decimal PricePerUnit { get; set; }
    
    public int? ChargingDurationMinutes { get; set; }
}