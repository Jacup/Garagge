using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceItems;
using Application.Vehicles;
using Domain.Entities.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceRecords.Create;

internal sealed class CreateServiceRecordsCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : ICommandHandler<CreateServiceRecordCommand, ServiceRecordDto>
{
    public async Task<Result<ServiceRecordDto>> Handle(CreateServiceRecordCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<ServiceRecordDto>(VehicleErrors.NotFound(request.VehicleId));

        if (vehicle.UserId != userContext.UserId)
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.Unauthorized);

        var typeExists = await dbContext.ServiceTypes
            .AnyAsync(t => t.Id == request.ServiceTypeId, cancellationToken);
        
        if (!typeExists)
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.ServiceTypeNotFound(request.ServiceTypeId));

        // Load the service type for mapping
        var serviceType = await dbContext.ServiceTypes
            .FirstAsync(t => t.Id == request.ServiceTypeId, cancellationToken);

        var serviceRecordId = Guid.NewGuid();
        var record = new ServiceRecord
        {
            Id = serviceRecordId,
            VehicleId = request.VehicleId,
            TypeId = request.ServiceTypeId,
            Type = serviceType,
            ServiceDate = request.ServiceDate,
            Title = request.Title,
            Notes = request.Notes,
            Mileage = request.Mileage,
            ManualCost = request.ManualCost
        };
        
        if (request.ServiceItems.Count > 0)
        {
            foreach (var itemCommand in request.ServiceItems)
            {
                var serviceItem = new ServiceItem
                {
                    ServiceRecordId = serviceRecordId,
                    Name = itemCommand.Name,
                    Type = itemCommand.Type,
                    UnitPrice = itemCommand.UnitPrice,
                    Quantity = itemCommand.Quantity,
                    PartNumber = itemCommand.PartNumber,
                    Notes = itemCommand.Notes
                };
                
                record.Items.Add(serviceItem);
            }
        }

        try
        {
            await dbContext.ServiceRecords.AddAsync(record, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<ServiceRecordDto>(ServiceRecordErrors.CreateFailed);
        }

        var dto = new ServiceRecordDto(
            record!.Id,
            record.Title,
            record.Notes,
            record.Mileage,
            record.ServiceDate,
            record.TotalCost,
            record.TypeId,
            record.Type!.Name,
            record.Items.Select(si => new ServiceItemDto(
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
            record.VehicleId,
            record.CreatedDate,
            record.UpdatedDate
        );

        return Result.Success(dto);
    }
}