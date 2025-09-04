using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.DeleteEnergyEntry;

public class DeleteEnergyEntryCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<DeleteEnergyEntryCommand>
{
    public async Task<Result> Handle(DeleteEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure(EnergyEntryErrors.Unauthorized);

        // Single query to find and validate the entry
        var energyEntry = await dbContext.EnergyEntries
            .Include(e => e.Vehicle)
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.VehicleId == request.VehicleId, cancellationToken);

        if (energyEntry is null)
            return Result.Failure(EnergyEntryErrors.NotFound(request.Id));

        // Check if vehicle belongs to user
        if (energyEntry.Vehicle?.UserId != userId)
            return Result.Failure(VehicleErrors.NotFound(request.VehicleId));

        try
        {
            dbContext.EnergyEntries.Remove(energyEntry);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(EnergyEntryErrors.DeleteFailed(request.Id));
        }
    }
}