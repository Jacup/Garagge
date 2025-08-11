using Application.Abstractions.Messaging;
using Application.Core;

namespace Application.Vehicles.GetMyVehicles;

public sealed record GetMyVehiclesQuery(Guid UserId, int Page, int PageSize, string? SearchTerm)
    : IQuery<PagedList<VehicleDto>>;