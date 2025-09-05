using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.GetEnergyEntriesByUser;

internal sealed class GetEnergyEntriesByUserQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : IQueryHandler<GetEnergyEntriesByUserQuery, PagedList<EnergyEntryDto>>
{
    public async Task<Result<PagedList<EnergyEntryDto>>> Handle(GetEnergyEntriesByUserQuery request, CancellationToken cancellationToken)
    {
        if (userContext.UserId != request.UserId)
            return Result.Failure<PagedList<EnergyEntryDto>>(EnergyEntryErrors.Unauthorized);

        var entriesQuery = dbContext.EnergyEntries
            .AsNoTracking()
            .Where(e => e.Vehicle!.UserId == request.UserId);


        if (request.EnergyType.HasValue && request.EnergyType.Value != EnergyType.None)
        {
            entriesQuery = entriesQuery
                .Where(ee => (request.EnergyType.Value & ee.Type) != 0);
        }

        entriesQuery = entriesQuery
             .OrderByDescending(ee => ee.Date)
             .ThenByDescending(ee => ee.Mileage);

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