using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.GetMyVehicleById;

internal sealed class GetMyVehicleByIdQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetMyVehicleByIdQuery, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(GetMyVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

        var vehicle = await context.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == request.UserId && v.Id == request.VehicleId)
            .Include(v => v.VehicleEnergyTypes)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle == null)
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound(request.VehicleId));

        var vehicleDto = vehicle.Adapt<VehicleDto>();
        
        return Result.Success(vehicleDto);
    }
}