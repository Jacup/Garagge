using Domain.Abstractions;

namespace Domain.Entities.Vehicles;

public sealed record VehicleRegisteredDomainEvent(Guid VehicleId) : IDomainEvent;
