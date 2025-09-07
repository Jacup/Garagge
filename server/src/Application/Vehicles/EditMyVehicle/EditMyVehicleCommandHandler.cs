using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.EditMyVehicle;

public class EditMyVehicleCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<EditMyVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(EditMyVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

        var vehicle = await dbContext.Vehicles.FirstOrDefaultAsync(v => v.UserId == userId && v.Id == request.VehicleId, cancellationToken);
        
        if (vehicle == null)
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound(request.VehicleId));

        try
        {
            vehicle.Brand = request.Brand;
            vehicle.Model = request.Model;
            vehicle.EngineType = request.EngineType;
            vehicle.ManufacturedYear = request.ManufacturedYear;
            vehicle.Type = request.Type;
            vehicle.VIN = request.VIN;
            
            dbContext.Vehicles.Update(vehicle);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<VehicleDto>(VehicleErrors.EditFailed(request.VehicleId));
        }

        return vehicle.Adapt<VehicleDto>();
    }
}