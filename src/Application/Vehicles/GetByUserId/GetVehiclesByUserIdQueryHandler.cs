using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.GetByUserId;

internal sealed class GetVehiclesByUserIdQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetVehiclesByUserIdQuery, ICollection<VehicleDto>>
{
    public async Task<Result<ICollection<VehicleDto>>> Handle(GetVehiclesByUserIdQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<ICollection<VehicleDto>>(VehicleErrors.Unauthorized);
        }

        var dtos = await context.Vehicles
            .Where(v => v.UserId == request.UserId)
            .Select(v => v.Adapt<VehicleDto>())
            .ToListAsync(cancellationToken: cancellationToken);

        if (dtos.Count == 0)
        {
            return Result.Failure<ICollection<VehicleDto>>(VehicleErrors.NotFound(request.UserId));
        }

        return dtos;
    }
}