using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Services.EnergyStats;
using Application.Vehicles;
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
            .Where(v => v.Id == request.VehicleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<EnergyStatsDto>(VehicleErrors.NotFound(request.VehicleId));

        if (vehicle.UserId != userContext.UserId)
            return Result.Failure<EnergyStatsDto>(EnergyEntryErrors.Unauthorized);

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
                TotalVolume: 0,
                AverageConsumption: 0,
                TotalCost: 0,
                AveragePricePerUnit: 0,
                EnergyTypes: []
            ));
        }
        
        var totalVolume = energyStatsService.CalculateTotalVolume(entries);
        var averageConsumption = energyStatsService.CalculateAverageConsumption(entries);
        var totalCost = energyStatsService.CalculateTotalCost(entries);
        var averagePricePerUnit = energyStatsService.CalculateAveragePricePerUnit(entries);
        var energyTypes = entries.Select(e => e.Type).Distinct().ToArray();

        var statsDto = new EnergyStatsDto(
            TotalVolume: totalVolume,
            AverageConsumption: averageConsumption,
            TotalCost: totalCost,
            AveragePricePerUnit: averagePricePerUnit,
            EnergyTypes: energyTypes
        );

        return Result.Success(statsDto);
    }
}