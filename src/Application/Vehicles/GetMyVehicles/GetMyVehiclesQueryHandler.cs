using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.GetMyVehicles;

internal sealed class GetMyVehiclesQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetMyVehiclesQuery, ICollection<VehicleDto>>
{
    public async Task<Result<ICollection<VehicleDto>>> Handle(GetMyVehiclesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
            return Result.Failure<ICollection<VehicleDto>>(VehicleErrors.Unauthorized);

        var vehicles = await context.Vehicles
            .Where(v => v.UserId == request.UserId)
            .ProjectToType<VehicleDto>()
            .ToListAsync(cancellationToken);

        if (vehicles.Count == 0)
            return Result.Failure<ICollection<VehicleDto>>(VehicleErrors.NotFoundForUser(request.UserId));

        return Result.Success<ICollection<VehicleDto>>(vehicles);
    }
}