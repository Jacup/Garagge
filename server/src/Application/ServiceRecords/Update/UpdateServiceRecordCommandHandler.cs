using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceItems;
using Application.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceRecords.Update;

internal sealed class UpdateServiceRecordCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<UpdateServiceRecordCommand, ServiceRecordDto>
{
    public async Task<Result<ServiceRecordDto>> Handle(UpdateServiceRecordCommand request, CancellationToken cancellationToken)
    {
        var serviceRecord = await dbContext.ServiceRecords
            .Include(sr => sr.Vehicle)
            .Include(sr => sr.Type)
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(
                sr => sr.Id == request.ServiceRecordId && sr.VehicleId == request.VehicleId,
                cancellationToken);

        if (serviceRecord is null)
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.NotFound(request.ServiceRecordId));

        if (serviceRecord.Vehicle is null)
            return Result.Failure<ServiceRecordDto>(VehicleErrors.NotFound(request.VehicleId));

        if (serviceRecord.Vehicle.UserId != userContext.UserId)
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.Unauthorized);

        var serviceType = await dbContext.ServiceTypes.FirstOrDefaultAsync(t => t.Id == request.ServiceTypeId, cancellationToken);

        if (serviceType is null)
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.ServiceTypeNotFound(request.ServiceTypeId));

        serviceRecord.Title = request.Title;
        serviceRecord.Notes = request.Notes;
        serviceRecord.Mileage = request.Mileage;
        serviceRecord.ServiceDate = request.ServiceDate;
        serviceRecord.ManualCost = request.ManualCost;
        serviceRecord.TypeId = request.ServiceTypeId;
        serviceRecord.Type = serviceType;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.UpdateFailed(request.ServiceRecordId));
        }

        var dto = new ServiceRecordDto(
            serviceRecord.Id,
            serviceRecord.Title,
            serviceRecord.Notes,
            serviceRecord.Mileage,
            serviceRecord.ServiceDate,
            serviceRecord.TotalCost,
            serviceRecord.TypeId,
            serviceRecord.Type.Name,
            serviceRecord.Items.Select(si => si.Adapt<ServiceItemDto>()).ToList(),
            serviceRecord.VehicleId,
            serviceRecord.CreatedDate,
            serviceRecord.UpdatedDate
        );

        return Result.Success(dto);
    }
}