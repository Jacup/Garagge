using Application.Dashboard.GetStats;

namespace Application.Abstractions;

public interface IStatisticsService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole);
}