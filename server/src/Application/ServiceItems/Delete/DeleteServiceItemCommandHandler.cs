using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceRecords;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceItems.Delete;

public class DeleteServiceItemCommandHandler(IApplicationDbContext dbContext, IUserContext userContext) : ICommandHandler<DeleteServiceItemCommand>
{
    public async Task<Result> Handle(DeleteServiceItemCommand request, CancellationToken cancellationToken)
    {
        var serviceItem = await dbContext.ServiceItems
            .Include(si => si.ServiceRecord)
            .ThenInclude(sr => sr!.Vehicle)
            .FirstOrDefaultAsync(
                si => si.Id == request.ServiceItemId 
                   && si.ServiceRecordId == request.ServiceRecordId
                   && si.ServiceRecord!.VehicleId == request.VehicleId,
                cancellationToken);

        if (serviceItem is null)
            return Result.Failure(ServiceItemsErrors.NotFound(request.ServiceItemId));
        
        if (serviceItem.ServiceRecord is null)
            return Result.Failure(ServiceRecordErrors.NotFound(request.ServiceRecordId));
        
        if (serviceItem.ServiceRecord.Vehicle is null)
            return Result.Failure(ServiceRecordErrors.NotFound(request.VehicleId));
        
        if (serviceItem.ServiceRecord.Vehicle.UserId != userContext.UserId)
            return Result.Failure(ServiceRecordErrors.Unauthorized);

        try
        {
            dbContext.ServiceItems.Remove(serviceItem);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure(ServiceItemsErrors.DeleteFailed(request.ServiceItemId));
        }

        return Result.Success();
    }
}