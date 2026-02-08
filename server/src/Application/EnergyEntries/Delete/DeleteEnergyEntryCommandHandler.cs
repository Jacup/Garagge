using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.Delete;

internal sealed class DeleteEnergyEntryCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<DeleteEnergyEntryCommand>
{
    public async Task<Result> Handle(DeleteEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var energyEntry = await dbContext.EnergyEntries
            .Include(e => e.Vehicle)
            .FirstOrDefaultAsync(e =>
                    e.Id == request.Id &&
                    e.VehicleId == request.VehicleId &&
                    e.Vehicle.UserId == userContext.UserId,
                cancellationToken);

        if (energyEntry is null)
            return Result.Failure(EnergyEntryErrors.NotFound);

        dbContext.EnergyEntries.Remove(energyEntry);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}