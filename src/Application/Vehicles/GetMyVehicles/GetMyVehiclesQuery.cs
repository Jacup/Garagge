using Application.Abstractions.Messaging;

namespace Application.Vehicles.GetMyVehicles;

public sealed record GetMyVehiclesQuery(Guid UserId) : IQuery<ICollection<VehicleDto>>;