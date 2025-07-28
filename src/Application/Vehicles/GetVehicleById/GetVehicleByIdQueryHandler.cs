using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.GetVehicleById;

internal sealed class GetVehicleByIdQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetVehicleByIdQuery, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

        var vehicle = await context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == request.UserId && v.Id == request.VehicleId, cancellationToken: cancellationToken);

        if (vehicle == null)
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound(request.VehicleId));

        return vehicle.Adapt<VehicleDto>();
    }
}