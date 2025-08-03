using Domain.Abstractions;

namespace Domain.Entities.EnergyEntries;

public class EnergyEntryRegisteredDomainEvent(Guid EnergyEntryId): IDomainEvent;