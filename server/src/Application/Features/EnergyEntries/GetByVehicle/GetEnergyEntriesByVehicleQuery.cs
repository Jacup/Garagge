using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Enums;

namespace Application.Features.EnergyEntries.GetByVehicle;

public sealed record GetEnergyEntriesByVehicleQuery(
    Guid VehicleId, 
    int Page, 
    int PageSize, 
    IReadOnlyCollection<EnergyType>? EnergyTypes = null) 
    : IQuery<PagedList<EnergyEntryDto>>;
