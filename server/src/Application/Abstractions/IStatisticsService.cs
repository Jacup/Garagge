using Application.Dashboard.GetStats;
using Application.Features.Vehicles.Stats;

namespace Application.Abstractions;

public interface IStatisticsService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole);
    Task<VehicleStatsDto> GetVehicleStats(Guid vehicleId);
}