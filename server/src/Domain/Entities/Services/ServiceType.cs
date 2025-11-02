namespace Domain.Entities.Services;

public sealed class ServiceType : Entity
{
    public required string Name { get; set; }
    
    public override string ToString() => Name;
}