using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Services.EnergyStats;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.GetStats;

internal sealed class GetEnergyStatsQueryHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IEnergyStatsService energyStatsService
) : IQueryHandler<GetEnergyStatsQuery, EnergyStatsDto>
{
    public async Task<Result<EnergyStatsDto>> Handle(GetEnergyStatsQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId && v.UserId == userContext.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<EnergyStatsDto>(EnergyEntryErrors.NotFound);

        var entriesQuery = dbContext.EnergyEntries
            .AsNoTracking()
            .Where(v => v.VehicleId == request.VehicleId);

        if (request.EnergyTypes.Length > 0)
        {
            entriesQuery = entriesQuery
                .Where(ee => request.EnergyTypes.Contains(ee.Type));
        }

        var entries = await entriesQuery.ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return Result.Success(new EnergyStatsDto(
                vehicle.Id,
                0,
                0,
                []));
        }

        var statisticsByUnit = entries
            .GroupBy(e => e.EnergyUnit)
            .Select(g => energyStatsService.CalculateStatisticsForUnit(
                unit: g.Key,
                entries: g.ToList()))
            .ToArray();

        var totalCost = energyStatsService.CalculateTotalCost(entries);

        return new EnergyStatsDto(vehicle.Id, totalCost, entries.Count, statisticsByUnit);
    }
}