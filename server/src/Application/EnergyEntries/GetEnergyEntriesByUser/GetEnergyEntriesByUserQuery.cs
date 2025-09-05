using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Enums;

namespace Application.EnergyEntries.GetEnergyEntriesByUser;

public sealed record GetEnergyEntriesByUserQuery(
    Guid UserId, 
    int Page, 
    int PageSize, 
    EnergyType? EnergyType) 
    : IQuery<PagedList<EnergyEntryDto>>;