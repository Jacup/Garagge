using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.VehicleEnergyTypes.Delete;

internal sealed class DeleteVehicleEnergyTypeCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : ICommandHandler<DeleteVehicleEnergyTypeCommand>
{
    public async Task<Result> Handle(DeleteVehicleEnergyTypeCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure(VehicleEnergyTypeErrors.Unauthorized);

        var vet = await dbContext.VehicleEnergyTypes
            .Where(vet =>
                vet.Id == request.Id &&
                dbContext.Vehicles.Any(v => v.Id == vet.VehicleId && v.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);

        if (vet == null)
            return Result.Failure(VehicleEnergyTypeErrors.NotFound(request.Id));

        var energyEntriesExists = await dbContext.EnergyEntries
            .AnyAsync(ee =>
                ee.VehicleId == vet.VehicleId &&
                ee.Type == vet.EnergyType, cancellationToken);

        if (energyEntriesExists)
            return Result.Failure(VehicleEnergyTypeErrors.DeleteFailedEntriesExists(request.Id));

        try
        {
            dbContext.VehicleEnergyTypes.Remove(vet);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.DeleteFailed(request.Id));
        }

        return Result.Success();
    }
}