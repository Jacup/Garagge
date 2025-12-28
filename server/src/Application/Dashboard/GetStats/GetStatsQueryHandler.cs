using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Users;

namespace Application.Dashboard.GetStats;

public class GetStatsQueryHandler(IUserContext userContext, IStatisticsService statisticsService) : IQueryHandler<GetStatsQuery, DashboardStatsDto>
{
    public async Task<Result<DashboardStatsDto>> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var userRole = "User";

        if (userId == Guid.Empty)
        {
            return Result.Failure<DashboardStatsDto>(UserErrors.Unauthorized);
        }

        var stats = await statisticsService.GetDashboardStatsAsync(userId, userRole);

        return stats;
    }
}