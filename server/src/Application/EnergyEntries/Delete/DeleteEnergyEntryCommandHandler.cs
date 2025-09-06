using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.Delete;

public class DeleteEnergyEntryCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<DeleteEnergyEntryCommand>
{
    public async Task<Result> Handle(DeleteEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure(EnergyEntryErrors.Unauthorized);

        var energyEntry = await dbContext.EnergyEntries
            .Include(e => e.Vehicle)
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.VehicleId == request.VehicleId, cancellationToken);

        if (energyEntry is null)
            return Result.Failure(EnergyEntryErrors.NotFound(request.Id));

        if (energyEntry.Vehicle?.UserId != userId)
            return Result.Failure(EnergyEntryErrors.NotFound(request.VehicleId));

        try
        {
            dbContext.EnergyEntries.Remove(energyEntry);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure(EnergyEntryErrors.DeleteFailed(request.Id));
        }

        return Result.Success();
    }
}