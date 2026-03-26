using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Features.ServiceItems;
using Application.Features.Vehicles;
using Domain.Entities.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ServiceRecords.Create;

internal sealed class CreateServiceRecordsCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : ICommandHandler<CreateServiceRecordCommand, ServiceRecordDto>
{
    public async Task<Result<ServiceRecordDto>> Handle(CreateServiceRecordCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId &&
                        v.UserId == userContext.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<ServiceRecordDto>(VehicleErrors.NotFound);

        var serviceRecordId = Guid.NewGuid();
        var record = new ServiceRecord
        {
            Id = serviceRecordId,
            VehicleId = request.VehicleId,
            Type = request.Type,
            ServiceDate = DateTime.SpecifyKind(request.ServiceDate, DateTimeKind.Utc),
            Title = request.Title,
            Notes = request.Notes,
            Mileage = request.Mileage,
            ManualCost = request.ManualCost
        };

        var items = request.ServiceItems.Select(item =>
            new ServiceItem
            {
                Id = Guid.NewGuid(),
                ServiceRecordId = serviceRecordId,
                Name = item.Name,
                Type = item.Type,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                PartNumber = item.PartNumber,
                Notes = item.Notes
            });

        foreach (ServiceItem serviceItem in items)
            record.Items.Add(serviceItem);

        await dbContext.ServiceRecords.AddAsync(record, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new ServiceRecordDto(
            record.Id,
            record.Title,
            record.Type,
            record.Notes,
            record.Mileage,
            record.ServiceDate,
            record.TotalCost,
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

        return dto;
    }
}