using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceRecords.Delete;

internal sealed class DeleteServiceRecordCommandHandler(IApplicationDbContext dbContext, IUserContext userContext) : ICommandHandler<DeleteServiceRecordCommand>
{
    public async Task<Result> Handle(DeleteServiceRecordCommand request, CancellationToken cancellationToken)
    {
        var serviceRecord = await dbContext.ServiceRecords
            .Include(sr => sr.Vehicle)
            .FirstOrDefaultAsync(sr =>
                    sr.Id == request.ServiceRecordId &&
                    sr.VehicleId == request.VehicleId,
                cancellationToken);

        if (serviceRecord?.Vehicle is null || serviceRecord.Vehicle.UserId != userContext.UserId)
            return Result.Failure(ServiceRecordErrors.NotFound);

        dbContext.ServiceRecords.Remove(serviceRecord);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}