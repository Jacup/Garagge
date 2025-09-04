using Domain.Enums;

namespace Application.EnergyEntries.Dtos;

public class ChargingEntryDto
{
    public Guid Id { get; init; }

    public DateTime CreatedDate { get; init; }
    public DateTime UpdatedDate { get; init; }

    public DateOnly Date { get; init; }
    public int Mileage { get; init; }
    public decimal Cost { get; init; }
    public Guid VehicleId { get; init; }

    public decimal EnergyAmount { get; init; }
    public EnergyUnit Unit { get; init; }
    public decimal PricePerUnit { get; init; }
    public int? ChargingDurationMinutes { get; init; }
}