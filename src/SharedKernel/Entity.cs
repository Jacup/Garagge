namespace SharedKernel;

public abstract class Entity
{
    public Guid Id { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    
    private readonly List<IDomainEvent> _domainEvents = [];

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
