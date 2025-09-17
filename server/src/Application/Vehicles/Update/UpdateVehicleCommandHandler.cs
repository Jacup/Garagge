using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.Update;

public class UpdateVehicleCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<UpdateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

        var vehicle = await dbContext.Vehicles.FirstOrDefaultAsync(v =>
                v.UserId == userId &&
                v.Id == request.VehicleId,
            cancellationToken);

        if (vehicle == null)
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound(request.VehicleId));

        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.EngineType = request.EngineType;
        vehicle.ManufacturedYear = request.ManufacturedYear;
        vehicle.Type = request.Type;
        vehicle.VIN = request.VIN;

        dbContext.Vehicles.Update(vehicle);
        
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<VehicleDto>(VehicleErrors.UpdateFailed(request.VehicleId));
        }

        return vehicle.Adapt<VehicleDto>();
    }
}