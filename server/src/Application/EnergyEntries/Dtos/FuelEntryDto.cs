using Domain.Enums;

namespace Application.EnergyEntries.Dtos;

public class FuelEntryDto
{
    public Guid Id { get; init; }
    
    public DateTime CreatedDate { get; init; }
    public DateTime UpdatedDate { get; init; }
    
    public DateOnly Date { get; init; }
    public int Mileage { get; init;  } 
    public decimal Cost { get; init; }
    public Guid VehicleId { get; init; }
    
    public decimal Volume { get; init; }
    public VolumeUnit Unit { get; init; }
    public decimal PricePerUnit { get; init; }
}