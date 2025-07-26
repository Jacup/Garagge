using SharedKernel;

namespace Domain.Vehicles;

public sealed record VehicleRegisteredDomainEvent(Guid VehicleId) : IDomainEvent;
