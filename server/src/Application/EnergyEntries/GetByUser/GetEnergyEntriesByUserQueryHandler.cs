using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.GetByUser;

internal sealed class GetEnergyEntriesByUserQueryHandler(
    IApplicationDbContext dbContext, 
    IUserContext userContext,
    IEnergyEntryFilterService filterService)
    : IQueryHandler<GetEnergyEntriesByUserQuery, PagedList<EnergyEntryDto>>
{
    public async Task<Result<PagedList<EnergyEntryDto>>> Handle(GetEnergyEntriesByUserQuery request, CancellationToken cancellationToken)
    {
        if (userContext.UserId != request.UserId)
            return Result.Failure<PagedList<EnergyEntryDto>>(EnergyEntryErrors.Unauthorized);

        var entriesQuery = dbContext.EnergyEntries
            .AsNoTracking()
            .Include(ee => ee.Vehicle)
            .AsQueryable();

        entriesQuery = filterService.ApplyUserFilter(entriesQuery, request.UserId);
        entriesQuery  = filterService.ApplyEnergyTypeFilter(entriesQuery, request.EnergyTypes);
        entriesQuery = filterService.ApplyDefaultSorting(entriesQuery);

        var energyEntriesDtoQuery = entriesQuery
            .Select(ee => new EnergyEntryDto(
                ee.Id,
                ee.VehicleId,
                ee.CreatedDate,
                ee.UpdatedDate,
                ee.Date,
                ee.Mileage,
                ee.Type,
                ee.EnergyUnit,
                ee.Volume,
                ee.Cost,
                ee.PricePerUnit
            ));


        var energyEntriesDto = await PagedList<EnergyEntryDto>.CreateAsync(
            energyEntriesDtoQuery,
            request.Page,
            request.PageSize);

        return Result.Success(energyEntriesDto);
    }
}