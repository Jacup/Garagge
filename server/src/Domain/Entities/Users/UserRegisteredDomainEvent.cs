using Domain.Abstractions;

namespace Domain.Entities.Users;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;
