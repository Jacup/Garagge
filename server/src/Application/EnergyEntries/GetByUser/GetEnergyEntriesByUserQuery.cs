using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Enums;

namespace Application.EnergyEntries.GetByUser;

public sealed record GetEnergyEntriesByUserQuery(
    Guid UserId, 
    int Page, 
    int PageSize, 
    IReadOnlyCollection<EnergyType>? EnergyTypes = null) 
    : IQuery<PagedList<EnergyEntryDto>>;