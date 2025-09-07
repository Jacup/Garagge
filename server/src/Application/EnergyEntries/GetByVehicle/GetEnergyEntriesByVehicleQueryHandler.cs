using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.GetByVehicle;

internal sealed class GetEnergyEntriesByVehicleQueryHandler(
    IApplicationDbContext dbContext, 
    IUserContext userContext,
    IEnergyEntryFilterService filterService)
    : IQueryHandler<GetEnergyEntriesByVehicleQuery, PagedList<EnergyEntryDto>>
{
    public async Task<Result<PagedList<EnergyEntryDto>>> Handle(GetEnergyEntriesByVehicleQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<PagedList<EnergyEntryDto>>(VehicleErrors.NotFound(request.VehicleId));

        if (vehicle.UserId != userContext.UserId)
            return Result.Failure<PagedList<EnergyEntryDto>>(EnergyEntryErrors.Unauthorized);

        var entriesQuery = dbContext.EnergyEntries.AsNoTracking();

        entriesQuery = filterService.ApplyVehicleFilter(entriesQuery, request.VehicleId);
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
