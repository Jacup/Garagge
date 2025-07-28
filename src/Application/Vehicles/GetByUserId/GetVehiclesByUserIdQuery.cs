using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetByUserId;

public sealed record GetVehiclesByUserIdQuery(Guid UserId) : IQuery<ICollection<VehicleDto>>;