using Domain.Enums;

namespace Domain.Entities.EnergyEntries;

public class EnergyUnitStats 
{
    public EnergyUnit Unit { get; set; }
    
    public ICollection<EnergyType> EnergyTypes { get; set; } = [];
    
    public int EntriesCount { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalCost { get; set; }
    
    public decimal AverageConsumption { get; set; }
    public decimal AveragePricePerUnit { get; set; }
    public decimal AverageCostPer100km { get; set; }
}