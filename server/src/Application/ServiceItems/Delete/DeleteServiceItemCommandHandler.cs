using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceItems.Delete;

internal sealed class DeleteServiceItemCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<DeleteServiceItemCommand>
{
    public async Task<Result> Handle(DeleteServiceItemCommand request, CancellationToken cancellationToken)
    {
        var serviceItem = await dbContext.ServiceItems
            .Include(si => si.ServiceRecord)
            .ThenInclude(sr => sr!.Vehicle)
            .FirstOrDefaultAsync(si => 
                    si.Id == request.ServiceItemId && 
                    si.ServiceRecordId == request.ServiceRecordId && 
                    si.ServiceRecord!.VehicleId == request.VehicleId,
                cancellationToken);

        if (serviceItem?.ServiceRecord?.Vehicle is null || serviceItem.ServiceRecord.Vehicle.UserId != userContext.UserId)
            return Result.Failure(ServiceItemsErrors.NotFound);
        
        dbContext.ServiceItems.Remove(serviceItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}