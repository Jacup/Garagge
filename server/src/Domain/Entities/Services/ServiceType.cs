namespace Domain.Entities.Services;

public sealed class ServiceType : Entity
{
    public required string Name { get; set; }
    
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    
    public override string ToString() => Name;
}