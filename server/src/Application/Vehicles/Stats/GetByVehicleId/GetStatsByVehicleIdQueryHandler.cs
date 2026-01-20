using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.Stats.GetByVehicleId;

public class GetStatsByVehicleIdQueryHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IStatisticsService statisticsService
) : IQueryHandler<GetStatsByVehicleIdQuery, VehicleStatsDto>
{
    public async Task<Result<VehicleStatsDto>> Handle(GetStatsByVehicleIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<VehicleStatsDto>(VehicleErrors.NotFound(request.VehicleId));

        if (vehicle.UserId != userContext.UserId)
            return Result.Failure<VehicleStatsDto>(VehicleErrors.Unauthorized);

        var dto = await statisticsService.GetVehicleStats(request.VehicleId);

        return Result.Success(dto);
    }
}