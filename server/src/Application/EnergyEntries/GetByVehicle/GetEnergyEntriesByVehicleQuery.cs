using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Enums;

namespace Application.EnergyEntries.GetByVehicle;

public sealed record GetEnergyEntriesByVehicleQuery(
    Guid VehicleId, 
    int Page, 
    int PageSize, 
    EnergyType? EnergyType = null) 
    : IQuery<PagedList<EnergyEntryDto>>;
