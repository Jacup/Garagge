using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetMyByUserId;

public sealed record GetVehiclesByUserIdQuery(Guid UserId) : IQuery<ICollection<VehicleDto>>;