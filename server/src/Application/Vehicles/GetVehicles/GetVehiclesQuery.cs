using Application.Abstractions.Messaging;
using Application.Core;

namespace Application.Vehicles.GetVehicles;

public sealed record GetVehiclesQuery(int PageSize, int Page,  string? SearchTerm)
    : IQuery<PagedList<VehicleDto>>;