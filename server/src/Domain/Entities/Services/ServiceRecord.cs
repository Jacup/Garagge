using Domain.Entities.Vehicles;
using Domain.Enums.Services;

namespace Domain.Entities.Services;

public sealed class ServiceRecord : Entity
{
    public required string Title { get; set; }
    public required ServiceRecordType Type { get; set; }
    public string? Notes { get; set; }
    
    public required DateTime ServiceDate { get; set; }
    public int? Mileage { get; set; }

    public decimal? ManualCost { get; set; }
    public decimal TotalCost => Items.Count != 0 ? 
        Items.Sum(x => x.TotalPrice) : 
        ManualCost ?? 0;

    public ICollection<ServiceItem> Items { get; set; } = new List<ServiceItem>();
    
    public required Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}