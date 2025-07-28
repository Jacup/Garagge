using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetMyVehicles;

public sealed record GetMyVehicles(Guid UserId) : IQuery<ICollection<VehicleDto>>;