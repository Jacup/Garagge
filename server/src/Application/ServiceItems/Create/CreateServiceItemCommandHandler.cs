using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceItems.Create;

internal sealed class CreateServiceItemCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<CreateServiceItemCommand, ServiceItemDto>
{
    public async Task<Result<ServiceItemDto>> Handle(CreateServiceItemCommand request, CancellationToken cancellationToken)
    {
        var serviceRecord = await dbContext.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Vehicle)
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRecordId, cancellationToken);

        if (serviceRecord?.Vehicle == null || serviceRecord.Vehicle.UserId != userContext.UserId)
            return Result.Failure<ServiceItemDto>(ServiceRecordErrors.NotFound);

        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = request.ServiceRecordId,
            Name = request.Name,
            Type = request.Type,
            UnitPrice = request.UnitPrice,
            Quantity = request.Quantity,
            PartNumber = request.PartNumber,
            Notes = request.Notes
        };

        await dbContext.ServiceItems.AddAsync(serviceItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return serviceItem.Adapt<ServiceItemDto>();
    }
}