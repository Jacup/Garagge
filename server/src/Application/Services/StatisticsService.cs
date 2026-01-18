using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Dashboard.GetStats;
using Application.Vehicles.Stats;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Services;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class StatisticsService(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : IStatisticsService
{
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole)
    {
        var vehiclesQuery = GetVehicleScope(userId, userRole);

        var fuelExpenses = await CalculateFuelExpenses(vehiclesQuery);
        var distanceDriven = await CalculateDistanceDriven(vehiclesQuery);
        var recentActivities = await GetRecentActivities(vehiclesQuery);

        return new DashboardStatsDto { FuelExpenses = fuelExpenses, DistanceDriven = distanceDriven, RecentActivity = recentActivities };
    }

    internal IQueryable<Vehicle> GetVehicleScope(Guid userId, string userRole)
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

        (ContextTrend trend, TrendMode trendMode, string diffText) = CalculateTrend(currentMonthCost, previousMonthCost, inverse: true, isPercentage: true);

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

        (ContextTrend trend, TrendMode trendMode, string diffText) = CalculateTrend(currentDistance, previousDistance, inverse: false, isPercentage: false);

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

    private static (ContextTrend Trend, TrendMode Mode, string DiffText) CalculateTrend(decimal current, decimal previous, bool inverse, bool isPercentage)
    {
        decimal diff = current - previous;
        decimal percentageDiff = 0;

        if (isPercentage)
        {
            if (previous != 0) percentageDiff = diff / previous;
            else if (current > 0) percentageDiff = 1;
        }

        var trend = current > previous ? ContextTrend.Up : (current < previous ? ContextTrend.Down : ContextTrend.None);

        var mode = TrendMode.Neutral;

        if (trend == ContextTrend.Up)
            mode = inverse ? TrendMode.Bad : TrendMode.Good;
        if (trend == ContextTrend.Down)
            mode = inverse ? TrendMode.Good : TrendMode.Bad;

        var sign = diff > 0 ? "+" : "";

        string diffText;
        if (isPercentage)
        {
            diffText = $"{sign}{percentageDiff:P0}";
        }
        else
        {
            diffText = $"{sign}{diff}";
        }

        return (trend, mode, diffText);
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
                TotalCost = s.ManualCost ?? s.Items.Sum(i => i.UnitPrice * i.Quantity),
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
            VehicleId = v.Id,
            Type = ActivityType.VehicleAdded,
            Date = v.CreatedDate,
            Vehicle = $"{v.Brand} {v.Model}",
            ActivityDetails = []
        }));

        activities.AddRange(updatedVehicles.Select(v => new TimelineActivityDto
        {
            VehicleId = v.Id,
            Type = ActivityType.VehicleUpdated,
            Date = v.Date,
            Vehicle = $"{v.Brand} {v.Model}",
            ActivityDetails = []
        }));

        activities.AddRange(services.Select(s => new TimelineActivityDto
        {
            VehicleId = s.Id,
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
            VehicleId = e.Id,
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

    public async Task<VehicleStatsDto> GetVehicleStats(Guid vehicleId)
    {
        var energyEntries = await FetchEnergyEntriesAsync(vehicleId);
        var serviceRecords = await FetchServiceRecordsAsync(vehicleId);

        if (energyEntries.Count == 0 && serviceRecords.Count == 0)
        {
            return CreateEmptyVehicleStats(vehicleId);
        }

        var mileageStats = CalculateMileageStats(energyEntries, serviceRecords);
        var costStats = CalculateCostStats(energyEntries, serviceRecords);
        var dateStats = CalculateDateStats(energyEntries, serviceRecords);
        var efficiencyStats = CalculateEfficiencyStatsByEnergyType(energyEntries);
        var vehicleActivities = await GetVehicleActivitiesAsync(vehicleId, energyEntries, serviceRecords);

        return new VehicleStatsDto
        (
            VehicleId: vehicleId,
            TotalCost: costStats.TotalCost,
            LastMileage: mileageStats.LastMileage,
            DistanceTraveled: mileageStats.DistanceDriven,
            TotalFuelCost: costStats.TotalFuelCost,
            FuelCostPerKm: costStats.FuelCostPerKm,
            TotalFuelEntries: energyEntries.Count,
            LastFuelEntryDate: dateStats.LastFuelEntryDate,
            TotalServicesCost: costStats.TotalServiceCost,
            TotalServiceRecords: serviceRecords.Count,
            LastServiceDate: dateStats.LastServiceDate,
            EfficiencyStats: efficiencyStats,
            VehicleActivities: vehicleActivities
        );
    }

    private async Task<List<EnergyEntry>> FetchEnergyEntriesAsync(Guid vehicleId)
    {
        return await dbContext.EnergyEntries
            .Where(ee => ee.VehicleId == vehicleId)
            .AsNoTracking()
            .OrderBy(e => e.Mileage)
            .ToListAsync();
    }

    private async Task<List<ServiceRecord>> FetchServiceRecordsAsync(Guid vehicleId)
    {
        return await dbContext.ServiceRecords
            .Where(sr => sr.VehicleId == vehicleId)
            .AsNoTracking()
            .ToListAsync();
    }

    private async Task<VehicleActivityDto[]> GetVehicleActivitiesAsync(
        Guid vehicleId,
        List<EnergyEntry> energyEntries,
        List<ServiceRecord> serviceRecords)
    {
        var activities = new List<VehicleActivityDto>();

        // Pobierz dane pojazdu - tylko CreatedDate i UpdatedDate
        var vehicleData = await dbContext.Vehicles
            .Where(v => v.Id == vehicleId)
            .AsNoTracking()
            .Select(v => new { v.CreatedDate, v.UpdatedDate })
            .FirstOrDefaultAsync();

        if (vehicleData != null)
        {
            activities.Add(new VehicleActivityDto { Type = ActivityType.VehicleAdded, Date = vehicleData.CreatedDate, ActivityDetails = [] });

            if (vehicleData.UpdatedDate != vehicleData.CreatedDate)
            {
                activities.Add(new VehicleActivityDto { Type = ActivityType.VehicleUpdated, Date = vehicleData.UpdatedDate, ActivityDetails = [] });
            }
        }

        activities.AddRange(serviceRecords
            .OrderByDescending(s => s.CreatedDate)
            .Take(5)
            .Select(s => new VehicleActivityDto
            {
                Type = ActivityType.ServiceAdded,
                Date = s.CreatedDate,
                ActivityDetails =
                [
                    new ActivityDetail("Service", s.Title),
                    new ActivityDetail("Cost", s.TotalCost.ToString("C"))
                ]
            }));

        activities.AddRange(energyEntries
            .OrderByDescending(e => e.CreatedDate)
            .Take(5)
            .Select(e => new VehicleActivityDto
            {
                Type = e.Type == EnergyType.Electric ? ActivityType.Charge : ActivityType.Refuel,
                Date = e.CreatedDate,
                ActivityDetails =
                [
                    new ActivityDetail("Fuel Type", e.Type.ToString()),
                    new ActivityDetail("Cost", (e.Cost ?? 0).ToString("C"))
                ]
            }));

        return activities
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToArray();
    }

    private static VehicleStatsDto CreateEmptyVehicleStats(Guid vehicleId)
    {
        return new VehicleStatsDto(vehicleId, 0, 0, 0, 0, 0, 0, null, 0, 0, null, [], []);
    }

    internal static (int FirstMileage, int LastMileage, int DistanceDriven) CalculateMileageStats(
        List<EnergyEntry> energyEntries,
        List<ServiceRecord> serviceRecords)
    {
        var maxFuelMileage = energyEntries.Count > 0 ? energyEntries.Max(e => e.Mileage) : 0;
        var minFuelMileage = energyEntries.Count > 0 ? energyEntries.Min(e => e.Mileage) : int.MaxValue;

        var maxServiceMileage = serviceRecords.Any(s => s.Mileage.HasValue)
            ? serviceRecords.Where(s => s.Mileage != null).Max(s => s.Mileage!.Value)
            : 0;
        var minServiceMileage = serviceRecords.Any(s => s.Mileage.HasValue)
            ? serviceRecords.Where(s => s.Mileage != null).Min(s => s.Mileage!.Value)
            : int.MaxValue;

        var lastMileage = Math.Max(maxFuelMileage, maxServiceMileage);
        var validMinMileage = Math.Min(minFuelMileage, minServiceMileage);
        var firstMileage = validMinMileage == int.MaxValue ? 0 : validMinMileage;
        var distanceDriven = (lastMileage > 0 && firstMileage > 0) ? lastMileage - firstMileage : 0;

        return (firstMileage, lastMileage, distanceDriven);
    }

    internal static (decimal TotalCost, decimal TotalFuelCost, decimal TotalServiceCost, decimal FuelCostPerKm) CalculateCostStats(
        List<EnergyEntry> energyEntries,
        List<ServiceRecord> serviceRecords)
    {
        var totalFuelCost = energyEntries.Sum(e => e.Cost ?? 0);
        var totalServiceCost = serviceRecords.Sum(s => s.TotalCost);
        var totalCost = totalFuelCost + totalServiceCost;
        var fuelCostPerKm = CalculateFuelCostPerDistanceUnit(energyEntries);

        return (totalCost, totalFuelCost, totalServiceCost, fuelCostPerKm);
    }

    internal static (DateOnly? LastFuelEntryDate, DateOnly? LastServiceDate) CalculateDateStats(
        List<EnergyEntry> energyEntries,
        List<ServiceRecord> serviceRecords)
    {
        var lastFuelEntryDate = energyEntries.Count != 0 ? (DateOnly?)energyEntries.Max(e => e.Date) : null;
        var lastServiceDate = serviceRecords.Count != 0
            ? (DateOnly?)DateOnly.FromDateTime(serviceRecords.Max(s => s.ServiceDate))
            : null;

        return (lastFuelEntryDate, lastServiceDate);
    }

    internal static List<EnergyEfficiencyStatDto> CalculateEfficiencyStatsByEnergyType(List<EnergyEntry> energyEntries)
    {
        var efficiencyStats = new List<EnergyEfficiencyStatDto>();

        foreach (var group in energyEntries.GroupBy(e => e.Type))
        {
            var groupList = group.ToList();
            var costPerKm = CalculateFuelCostPerDistanceUnit(groupList);
            var avgConsumption = CalculateAverageConsumption(groupList);
            var totalFuelTypeCost = group.Sum(x => x.Cost ?? 0);
            var unit = group.First().EnergyUnit.ToString();

            efficiencyStats.Add(new EnergyEfficiencyStatDto(
                FuelType: group.Key,
                EnergyUnit: unit,
                AverageConsumption: avgConsumption,
                CostPerKm: costPerKm,
                TotalCost: totalFuelTypeCost,
                EntriesCount: group.Count()
            ));
        }

        return efficiencyStats;
    }

    internal static double CalculateAverageConsumption(List<EnergyEntry> entries)
    {
        if (entries.Count < 2)
            return 0;

        var sortedEntries = entries.OrderBy(e => e.Mileage).ToList();
        var distance = CalculateDistanceBetweenEntries(sortedEntries.First(), sortedEntries.Last());

        if (distance <= 0)
            return 0;

        var consumedVolume = sortedEntries
            .Take(sortedEntries.Count - 1)
            .Sum(e => e.Volume);

        return Math.Round((double)consumedVolume / distance * 100, 2);
    }

    private static int CalculateDistanceBetweenEntries(EnergyEntry first, EnergyEntry last)
    {
        var distance = last.Mileage - first.Mileage;
        return distance > 0 ? distance : 0;
    }

    private static decimal CalculateFuelCostPerDistanceUnit(List<EnergyEntry> entries)
    {
        if (entries.Count < 2)
            return 0m;

        var sortedEntries = entries.OrderBy(e => e.Mileage).ToList();
        var firstEntry = sortedEntries[0];
        var lastEntry = sortedEntries[^1];

        var distance = CalculateDistanceBetweenEntries(firstEntry, lastEntry);

        if (distance <= 0)
            return 0m;

        var realizedCost = CalculateRealizedCost(sortedEntries);

        return Math.Round(realizedCost / distance, 2);
    }

    private static decimal CalculateRealizedCost(List<EnergyEntry> sortedEntries)
    {
        var totalCost = sortedEntries.Sum(e => e.Cost.GetValueOrDefault());
        var lastEntryCost = sortedEntries[^1].Cost.GetValueOrDefault();

        return totalCost - lastEntryCost;
    }
}