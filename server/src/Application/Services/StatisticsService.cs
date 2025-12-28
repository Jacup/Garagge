using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Dashboard.GetStats;
using Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class StatisticsService(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : IStatisticsService
{
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole)
    {
        var vehiclesQuery = GetVehicleScope(userId, userRole);

        var fuelTask = CalculateFuelExpenses(vehiclesQuery);
        var distanceTask = CalculateDistanceDriven(vehiclesQuery);

        await Task.WhenAll(fuelTask, distanceTask);

        return new DashboardStatsDto(
            FuelExpenses: fuelTask.Result,
            DistanceDriven: distanceTask.Result
        );
    }

    private IQueryable<Vehicle> GetVehicleScope(Guid userId, string userRole)
    {
        return userRole switch
        {
            "User" => dbContext.Vehicles.AsNoTracking().Where(v => v.UserId == userId),
            _ => throw new NotImplementedException("Not supported role")
        };
    }

    private async Task<StatMetricDto> CalculateFuelExpenses(IQueryable<Vehicle> scope)
    {
        var now = dateTimeProvider.UtcNow;
        var currentMonthStart = new DateOnly(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var expenses = await scope
            .SelectMany(v => v.EnergyEntries)
            .Where(ee => ee.Date >= previousMonthStart)
            .Select(ee => new { ee.Date, Cost = ee.Cost ?? 0 })
            .ToListAsync();

        var currentMonthTotal = expenses
            .Where(ee => ee.Date >= currentMonthStart)
            .Sum(ee => ee.Cost);

        var previousMonthTotal = expenses
            .Where(ee => ee.Date >= previousMonthStart && ee.Date < currentMonthStart)
            .Sum(ee => ee.Cost);

        decimal percentageDiff = 0;
        if (previousMonthTotal != 0)
        {
            percentageDiff = (currentMonthTotal - previousMonthTotal) / previousMonthTotal;
        }
        else if (currentMonthTotal > 0)
        {
            percentageDiff = 1;
        }

        ContextTrend? trend;

        if (currentMonthTotal > previousMonthTotal)
            trend = ContextTrend.Up;
        else if (currentMonthTotal < previousMonthTotal)
            trend = ContextTrend.Down;
        else
            trend = ContextTrend.None;
        
        var trendMode = trend switch
        {
            ContextTrend.Up => TrendMode.Bad,
            ContextTrend.Down => TrendMode.Good,
            _ => TrendMode.Neutral
        };

        return new StatMetricDto
        {
            Value = currentMonthTotal.ToString("C"),
            Subtitle = "This month",
            ContextValue = (percentageDiff > 0 ? "+" : "") + percentageDiff.ToString("P0"),
            ContextAppendText = "vs last month",
            ContextTrend = trend,
            ContextTrendMode = trendMode
        };
    }

    private async Task<StatMetricDto> CalculateDistanceDriven(IQueryable<Vehicle> scope)
    {
        var now = dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var last30DaysStart = today.AddDays(-30);
        var previous30DaysStart = last30DaysStart.AddDays(-30);
        
        var historyStart = previous30DaysStart.AddMonths(-1);

        var entries = await scope
            .SelectMany(v => v.EnergyEntries)
            .Where(ee => ee.Date >= historyStart)
            .Select(ee => new { ee.VehicleId, ee.Date, ee.Mileage })
            .ToListAsync();

        var grouped = entries.GroupBy(e => e.VehicleId);

        long currentDistance = 0;
        long previousDistance = 0;

        foreach (var vehicleEntries in grouped)
        {
            var sorted = vehicleEntries.OrderBy(e => e.Date).ToList();

            for (int i = 1; i < sorted.Count; i++)
            {
                var diff = sorted[i].Mileage - sorted[i - 1].Mileage;
                if (diff < 0) diff = 0;

                var entryDate = sorted[i].Date;

                if (entryDate > last30DaysStart && entryDate <= today)
                {
                    currentDistance += diff;
                }
                else if (entryDate > previous30DaysStart && entryDate <= last30DaysStart)
                {
                    previousDistance += diff;
                }
            }
        }

        long diffDistance = currentDistance - previousDistance;

        var trend = currentDistance > previousDistance ? ContextTrend.Up :
                    currentDistance < previousDistance ? ContextTrend.Down :
                    ContextTrend.None;

        var trendMode = trend switch
        {
            ContextTrend.Up => TrendMode.Bad,
            ContextTrend.Down => TrendMode.Good,
            _ => TrendMode.Neutral
        };

        return new StatMetricDto
        {
            Value = $"{currentDistance} km",
            Subtitle = "Last 30 days",
            ContextValue = (diffDistance > 0 ? "+" : "") + $"{diffDistance} km",
            ContextAppendText = "vs previous 30 days",
            ContextTrend = trend,
            ContextTrendMode = trendMode
        };
    }
}