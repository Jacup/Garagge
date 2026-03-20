using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Services.EnergyStats;
using Domain.Enums.Energy;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.EnergyEntries.GetStats;

internal sealed class GetEnergyStatsQueryHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IEnergyStatsService energyStatsService,
    IDateTimeProvider dateTimeProvider
) : IQueryHandler<GetEnergyStatsQuery, EnergyStatsDto>
{
    public async Task<Result<EnergyStatsDto>> Handle(GetEnergyStatsQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId && v.UserId == userContext.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<EnergyStatsDto>(EnergyEntryErrors.VehicleNotFound);

        var entriesQuery = dbContext.EnergyEntries
            .AsNoTracking()
            .Where(e => e.VehicleId == request.VehicleId);

        var dateFrom = GetDateFrom(request.Period);

        if (dateFrom is not null)
            entriesQuery = entriesQuery.Where(e => e.Date >= dateFrom.Value);

        var entries = await entriesQuery
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Mileage)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return new EnergyStatsDto(
                VehicleId: vehicle.Id,
                TotalFuelCost: 0,
                TotalEntries: 0,
                DistanceDriven: 0,
                StatsByType: [],
                ChartEntries: []
            );
        }

        return new EnergyStatsDto(
            VehicleId: vehicle.Id,
            TotalFuelCost: energyStatsService.CalculateTotalCost(entries),
            TotalEntries: entries.Count,
            DistanceDriven: energyStatsService.CalculateDistanceDriven(entries),
            StatsByType: energyStatsService.CalculateStatsByType(entries),
            ChartEntries: entries.Select(e => new EnergyEntryDto(
                Id: e.Id,
                VehicleId: e.VehicleId,
                CreatedDate: e.CreatedDate,
                UpdatedDate: e.UpdatedDate,
                Date: e.Date,
                Mileage: e.Mileage,
                Type: e.Type,
                EnergyUnit: e.EnergyUnit,
                Volume: e.Volume,
                Cost: e.Cost,
                PricePerUnit: e.PricePerUnit,
                Consumption: 0
            )).ToArray()
        );
    }

    private DateOnly? GetDateFrom(StatsPeriod period) => period switch
    {
        StatsPeriod.Week => DateOnly.FromDateTime(dateTimeProvider.UtcNow).AddDays(-7),
        StatsPeriod.Month => DateOnly.FromDateTime(dateTimeProvider.UtcNow).AddMonths(-1),
        StatsPeriod.Year => DateOnly.FromDateTime(dateTimeProvider.UtcNow).AddYears(-1),
        _ => null
    };
}