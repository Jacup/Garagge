using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceItems.Create;

internal sealed class CreateServiceItemCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext) : ICommandHandler<CreateServiceItemCommand, ServiceItemDto>
{
    public async Task<Result<ServiceItemDto>> Handle(CreateServiceItemCommand request, CancellationToken cancellationToken)
    {
        var serviceRecord = await dbContext.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Vehicle)
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRecordId, cancellationToken);

        if (serviceRecord is null)
            return Result.Failure<ServiceItemDto>(ServiceRecordErrors.NotFound(request.ServiceRecordId));

        if (serviceRecord.Vehicle?.UserId != userContext.UserId)
            return Result.Failure<ServiceItemDto>(ServiceItemsErrors.Unauthorized);

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

        try
        {
            await dbContext.ServiceItems.AddAsync(serviceItem, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<ServiceItemDto>(ServiceItemsErrors.CreateFailed);
        }

        return Result.Success(serviceItem.Adapt<ServiceItemDto>());
    }
}