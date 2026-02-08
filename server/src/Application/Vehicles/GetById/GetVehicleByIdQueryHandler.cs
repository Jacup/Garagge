using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.GetById;

internal sealed class GetVehicleByIdQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetVehicleByIdQuery, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await context.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == userContext.UserId && v.Id == request.VehicleId)
            .Include(v => v.VehicleEnergyTypes)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle == null)
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound);

        return vehicle.Adapt<VehicleDto>();
    }
}