using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceRecords;
using Application.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceItems.Update;

internal sealed class UpdateServiceItemCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<UpdateServiceItemCommand, ServiceItemDto>
{
    public async Task<Result<ServiceItemDto>> Handle(UpdateServiceItemCommand request, CancellationToken cancellationToken)
    {
        var serviceItem = await dbContext.ServiceItems
            .Include(si => si.ServiceRecord)
            .ThenInclude(sr => sr!.Vehicle)
            .FirstOrDefaultAsync(
                si => si.Id == request.ServiceItemId
                      && si.ServiceRecordId == request.ServiceRecordId,
                cancellationToken);

        if (serviceItem is null)
            return Result.Failure<ServiceItemDto>(ServiceItemsErrors.NotFound(request.ServiceItemId));

        if (serviceItem.ServiceRecord is null)
            return Result.Failure<ServiceItemDto>(ServiceRecordErrors.NotFound(request.ServiceRecordId));

        if (serviceItem.ServiceRecord.Vehicle is null)
            return Result.Failure<ServiceItemDto>(VehicleErrors.NotFound(serviceItem.ServiceRecord.VehicleId));

        if (serviceItem.ServiceRecord.Vehicle.UserId != userContext.UserId)
            return Result.Failure<ServiceItemDto>(ServiceItemsErrors.Unauthorized);

        serviceItem.Name = request.Name;
        serviceItem.Type = request.Type;
        serviceItem.UnitPrice = request.UnitPrice;
        serviceItem.Quantity = request.Quantity;
        serviceItem.PartNumber = request.PartNumber;
        serviceItem.Notes = request.Notes;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failure<ServiceItemDto>(ServiceItemsErrors.UpdateFailed(request.ServiceItemId));
        }

        return Result.Success(serviceItem.Adapt<ServiceItemDto>());
    }
}