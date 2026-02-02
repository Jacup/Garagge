using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceItems;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceRecords.GetById;

internal sealed class GetServiceRecordByIdQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetServiceRecordByIdQuery, ServiceRecordDto>
{
    public async Task<Result<ServiceRecordDto>> Handle(GetServiceRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var serviceRecord = await context.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Vehicle)
            .Include(sr => sr.Type)
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => 
                    sr.Id == request.ServiceRecordId && 
                    sr.VehicleId == request.VehicleId, 
                cancellationToken);

        if (serviceRecord?.Vehicle == null || serviceRecord.Vehicle.UserId != userId)
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.NotFound);

        var dto = new ServiceRecordDto(
            serviceRecord.Id,
            serviceRecord.Title,
            serviceRecord.Notes,
            serviceRecord.Mileage,
            serviceRecord.ServiceDate,
            serviceRecord.TotalCost,
            serviceRecord.TypeId,
            serviceRecord.Type?.Name ?? string.Empty,
            serviceRecord.Items.Select(si => new ServiceItemDto(
                si.Id,
                si.Name,
                si.Type,
                si.UnitPrice,
                si.Quantity,
                si.TotalPrice,
                si.PartNumber,
                si.Notes,
                si.ServiceRecordId,
                si.CreatedDate,
                si.UpdatedDate)).ToList(),
            serviceRecord.VehicleId,
            serviceRecord.CreatedDate,
            serviceRecord.UpdatedDate
        );

        return dto;
    }
}