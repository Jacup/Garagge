using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.Delete;

internal sealed class DeleteVehicleByIdCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<DeleteVehicleByIdCommand>
{
    public async Task<Result> Handle(DeleteVehicleByIdCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty)
            return Result.Failure(VehicleErrors.Unauthorized);

        var vehicle = await dbContext.Vehicles
            .FirstOrDefaultAsync(v =>
                    v.Id == request.VehicleId &&
                    v.UserId == userId,
                cancellationToken);

        if (vehicle is null)
            return Result.Failure(VehicleErrors.NotFound(request.VehicleId));

        dbContext.Vehicles.Remove(vehicle);
        
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure(VehicleErrors.DeleteFailed(request.VehicleId));
        }

        return Result.Success();
    }
}