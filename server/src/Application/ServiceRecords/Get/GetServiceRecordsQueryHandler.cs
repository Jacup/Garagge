using Application.Abstractions.Authentication; 
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Application.ServiceItems;
using Application.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace Application.ServiceRecords.Get;

internal sealed class GetServiceRecordsQueryHandler(
    IApplicationDbContext dbContext, 
    IUserContext userContext,
    IServiceRecordFilterService filterService)
    : IQueryHandler<GetServiceRecordsQuery, PagedList<ServiceRecordDto>>
{
    public async Task<Result<PagedList<ServiceRecordDto>>> Handle(GetServiceRecordsQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<PagedList<ServiceRecordDto>>(VehicleErrors.NotFound(request.VehicleId));

        if (vehicle.UserId != userContext.UserId)
            return Result.Failure<PagedList<ServiceRecordDto>>(ServiceRecordErrors.Unauthorized);

        var serviceRecordsQuery = dbContext.ServiceRecords
            .AsNoTracking()
            .Include(sr => sr.Type)
            .Include(sr => sr.Items)
            .Where(sr => sr.VehicleId == request.VehicleId);

        serviceRecordsQuery = filterService.ApplyFilters(serviceRecordsQuery, request);

        serviceRecordsQuery = filterService.ApplySorting(serviceRecordsQuery, request.SortBy, request.SortDescending);

        var serviceRecordsDtoQuery = serviceRecordsQuery
            .Select(sr => new ServiceRecordDto(
                sr.Id,
                sr.Title,
                sr.Notes,
                sr.Mileage,
                sr.ServiceDate,
                sr.TotalCost,
                sr.TypeId,
                sr.Type!.Name,
                sr.Items.Select(si => new ServiceItemDto(
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
                sr.VehicleId,
                sr.CreatedDate,
                sr.UpdatedDate
            ));

        PagedList<ServiceRecordDto> serviceRecordsDto;
        
        if (filterService.RequiresInMemorySorting(request.SortBy))
        {
            var totalCount = await serviceRecordsDtoQuery.CountAsync(cancellationToken);
            var allItems = await serviceRecordsDtoQuery.ToListAsync(cancellationToken);
            
            var sortedItems = request.SortDescending
                ? allItems.OrderByDescending(sr => sr.TotalCost).ToList()
                : allItems.OrderBy(sr => sr.TotalCost).ToList();
            
            var pagedItems = sortedItems
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
            
            serviceRecordsDto = new PagedList<ServiceRecordDto>(pagedItems, request.Page, request.PageSize, totalCount);
        }
        else
        {
            serviceRecordsDto = await PagedList<ServiceRecordDto>.CreateAsync(
                serviceRecordsDtoQuery,
                request.Page,
                request.PageSize);
        }

        return Result.Success(serviceRecordsDto);
    }
}
