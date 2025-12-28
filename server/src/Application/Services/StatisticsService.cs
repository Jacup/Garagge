using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Dashboard.GetStats;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class StatisticsService(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : IStatisticsService
{
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole)
    {
        var vehiclesQuery = GetVehicleScope(userId, userRole);

        var fuelTask = CalculateFuelExpenses(vehiclesQuery);
        var distanceTask = CalculateDistanceDriven(vehiclesQuery);
        var recentActivitiesTask = GetRecentActivities(vehiclesQuery);

        await Task.WhenAll(fuelTask, distanceTask, recentActivitiesTask);

        return new DashboardStatsDto { FuelExpenses = await fuelTask, DistanceDriven = await distanceTask, RecentActivity = await recentActivitiesTask };
    }

    private IQueryable<Vehicle> GetVehicleScope(Guid userId, string userRole)
    {
        var query = dbContext.Vehicles.AsNoTracking();

        return userRole switch
        {
            "User" => query.Where(v => v.UserId == userId),
            _ => query.Where(v => false)
        };
    }

    private async Task<StatMetricDto> CalculateFuelExpenses(IQueryable<Vehicle> scope)
    {
        var now = dateTimeProvider.UtcNow;
        var currentMonthStart = new DateOnly(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var currentMonthCost = await scope
            .SelectMany(v => v.EnergyEntries)
            .Where(e => e.Date >= currentMonthStart)
            .SumAsync(e => e.Cost ?? 0);

        var previousMonthCost = await scope
            .SelectMany(v => v.EnergyEntries)
            .Where(e => e.Date >= previousMonthStart && e.Date < currentMonthStart)
            .SumAsync(e => e.Cost ?? 0);

        (ContextTrend trend, TrendMode trendMode, string diffText) = CalculateTrend(currentMonthCost, previousMonthCost, inverse: true);

        return new StatMetricDto
        {
            Value = currentMonthCost.ToString("C"),
            Subtitle = "This month",
            ContextValue = diffText,
            ContextAppendText = "vs last month",
            ContextTrend = trend,
            ContextTrendMode = trendMode
        };
    }

    private async Task<StatMetricDto> CalculateDistanceDriven(IQueryable<Vehicle> scope)
    {
        var now = dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var startOfCurrentPeriod = today.AddDays(-30);
        var startOfPreviousPeriod = startOfCurrentPeriod.AddDays(-30);

        var entries = await scope
            .SelectMany(v => v.EnergyEntries)
            .Where(ee => ee.Date >= startOfPreviousPeriod)
            .Select(ee => new { ee.VehicleId, ee.Date, ee.Mileage })
            .OrderBy(e => e.Date)
            .ToListAsync();

        long currentDistance = 0;
        long previousDistance = 0;

        foreach (var group in entries.GroupBy(e => e.VehicleId))
        {
            var sorted = group.ToList();

            for (int i = 1; i < sorted.Count; i++)
            {
                var diff = sorted[i].Mileage - sorted[i - 1].Mileage;
                if (diff < 0) continue;

                var date = sorted[i].Date;

                if (date > startOfCurrentPeriod)
                    currentDistance += diff;
                else if (date > startOfPreviousPeriod)
                    previousDistance += diff;
            }
        }

        (ContextTrend trend, TrendMode trendMode, string diffText) = CalculateTrend(currentDistance, previousDistance, inverse: false);

        return new StatMetricDto
        {
            Value = $"{currentDistance} km",
            Subtitle = "Last 30 days",
            ContextValue = diffText + " km",
            ContextAppendText = "vs previous 30 days",
            ContextTrend = trend,
            ContextTrendMode = trendMode
        };
    }

    private static (ContextTrend Trend, TrendMode Mode, string DiffText) CalculateTrend(decimal current, decimal previous, bool inverse)
    {
        decimal diff = 0;
        if (previous != 0) diff = (current - previous) / previous;
        else if (current > 0) diff = 1;

        var trend = current > previous ? ContextTrend.Up : (current < previous ? ContextTrend.Down : ContextTrend.None);

        var mode = TrendMode.Neutral;

        if (trend == ContextTrend.Up)
            mode = inverse ? TrendMode.Bad : TrendMode.Good;
        if (trend == ContextTrend.Down)
            mode = inverse ? TrendMode.Good : TrendMode.Bad;

        var sign = diff > 0 ? "+" : "";
        return (trend, mode, $"{sign}{diff:P0}");
    }

    private async Task<List<TimelineActivityDto>> GetRecentActivities(IQueryable<Vehicle> scope)
    {
        var createdVehicles = await scope
            .OrderByDescending(v => v.CreatedDate)
            .Take(5)
            .Select(v => new { v.Id, v.CreatedDate, v.Brand, v.Model })
            .ToListAsync();

        var updatedVehicles = await scope
            .Where(v => v.UpdatedDate != v.CreatedDate)
            .OrderByDescending(v => v.UpdatedDate)
            .Take(5)
            .Select(v => new { v.Id, Date = v.UpdatedDate, v.Brand, v.Model })
            .ToListAsync();

        var services = await scope
            .SelectMany(v => v.ServiceRecords)
            .OrderByDescending(s => s.CreatedDate)
            .Take(5)
            .Select(s => new
            {
                s.Id,
                s.CreatedDate,
                VehicleBrand = s.Vehicle!.Brand,
                VehicleModel = s.Vehicle.Model,
                TotalCost = s.ManualCost ?? s.Items.Sum(i => i.TotalPrice),
                s.Title
            })
            .ToListAsync();

        var energy = await scope
            .SelectMany(v => v.EnergyEntries)
            .OrderByDescending(e => e.CreatedDate)
            .Take(5)
            .Select(e => new
            {
                e.Id,
                e.CreatedDate,
                VehicleBrand = e.Vehicle!.Brand,
                VehicleModel = e.Vehicle.Model,
                Cost = e.Cost ?? 0,
                e.Type
            })
            .ToListAsync();

        var activities = new List<TimelineActivityDto>();

        activities.AddRange(createdVehicles.Select(v => new TimelineActivityDto
        {
            Id = v.Id,
            Type = ActivityType.VehicleAdded,
            Date = v.CreatedDate,
            Vehicle = $"{v.Brand} {v.Model}",
            ActivityDetails = []
        }));

        activities.AddRange(updatedVehicles.Select(v => new TimelineActivityDto
        {
            Id = v.Id,
            Type = ActivityType.VehicleUpdated,
            Date = v.Date,
            Vehicle = $"{v.Brand} {v.Model}",
            ActivityDetails = []
        }));

        activities.AddRange(services.Select(s => new TimelineActivityDto
        {
            Id = s.Id,
            Type = ActivityType.ServiceAdded,
            Date = s.CreatedDate,
            Vehicle = $"{s.VehicleBrand} {s.VehicleModel}",
            ActivityDetails =
            [
                new ActivityDetail("Service", s.Title),
                new ActivityDetail("Cost", s.TotalCost.ToString("C"))
            ]
        }));

        activities.AddRange(energy.Select(e => new TimelineActivityDto
        {
            Id = e.Id,
            Type = e.Type == EnergyType.Electric ? ActivityType.Charge : ActivityType.Refuel,
            Date = e.CreatedDate,
            Vehicle = $"{e.VehicleBrand} {e.VehicleModel}",
            ActivityDetails =
            [
                new ActivityDetail("Cost", e.Cost.ToString("C"))
            ]
        }));

        return activities
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToList();
    }
}