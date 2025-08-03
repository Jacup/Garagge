using Domain.Enums;

namespace Domain.Entities.EnergyEntries;

public sealed class ChargingEntry : EnergyEntry
{
    public decimal EnergyAmount { get; set; }
    public EnergyUnit Unit { get; set; } = EnergyUnit.kWh;
    public decimal PricePerUnit { get; set; }
    
    public int? ChargingDurationMinutes { get; set; }
}