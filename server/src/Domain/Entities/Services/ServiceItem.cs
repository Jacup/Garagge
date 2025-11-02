using Domain.Enums.Services;

namespace Domain.Entities.Services;

public sealed class ServiceItem : Entity
{
    public required string Name { get; set; }
    public required ServiceItemType Type { get; set; }
    
    public required decimal UnitPrice { get; set; }
    public required decimal Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    
    public string? PartNumber { get; set; }
    public string? Notes { get; set; }
    
    public required Guid ServiceRecordId { get; set; }
    public ServiceRecord? ServiceRecord { get; set; }
}