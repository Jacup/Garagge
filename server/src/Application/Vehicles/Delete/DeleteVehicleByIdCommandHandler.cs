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
        var vehicle = await dbContext.Vehicles
            .FirstOrDefaultAsync(v =>
                    v.Id == request.VehicleId &&
                    v.UserId == userContext.UserId,
                cancellationToken);

        if (vehicle is null)
            return Result.Failure(VehicleErrors.NotFound);

        dbContext.Vehicles.Remove(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}