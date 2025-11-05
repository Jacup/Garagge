using Domain.Entities.Vehicles;

namespace Domain.Entities.Services;

public sealed class ServiceRecord : Entity
{
    public required string Title { get; set; }
    public string? Notes { get; set; }
    
    public int? Mileage { get; set; }
    public required DateTime ServiceDate { get; set; }

    public decimal? ManualCost { get; set; }
    public decimal TotalCost => Items.Count != 0 ? 
        Items.Sum(x => x.TotalPrice) : 
        ManualCost ?? 0;

    public ICollection<ServiceItem> Items { get; set; } = new List<ServiceItem>();

    public required Guid TypeId { get; set; }
    public ServiceType? Type { get; set; }
    
    public required Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}