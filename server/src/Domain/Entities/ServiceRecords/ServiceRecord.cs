using Domain.Entities.Vehicles;
using Domain.Enums;

namespace Domain.Entities.ServiceRecords;

public sealed class ServiceRecord : Entity
{
    public required ServiceCategory Category { get; set; }
    public required DateTime ServiceDate { get; set; }
    public int? Mileage { get; set; }
    public required decimal Cost { get; set; }
    public string? Notes { get; set; }
    
    public required Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}