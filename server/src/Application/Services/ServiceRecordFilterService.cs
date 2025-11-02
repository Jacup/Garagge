using Application.Abstractions.Services;
using Application.ServiceRecords.Get;
using Domain.Entities.Services;

namespace Application.Services;

internal sealed class ServiceRecordFilterService : IServiceRecordFilterService
{
    public IQueryable<ServiceRecord> ApplyFilters(IQueryable<ServiceRecord> query, GetServiceRecordsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(sr => 
                sr.Title.Contains(request.SearchTerm) || 
                (sr.Notes != null && sr.Notes.Contains(request.SearchTerm)));
        }
        
        if (request.ServiceTypeId.HasValue)
        {
            query = query.Where(sr => sr.TypeId == request.ServiceTypeId.Value);
        }
        
        if (request.DateFrom.HasValue)
        {
            query = query.Where(sr => sr.ServiceDate >= request.DateFrom.Value);
        }
        
        if (request.DateTo.HasValue)
        {
            query = query.Where(sr => sr.ServiceDate <= request.DateTo.Value);
        }
        
        return query;
    }
    
    public IQueryable<ServiceRecord> ApplySorting(IQueryable<ServiceRecord> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(sr => sr.ServiceDate);
        }
        
        return sortBy.ToLowerInvariant() switch
        {
            "servicedate" => descending 
                ? query.OrderByDescending(sr => sr.ServiceDate)
                : query.OrderBy(sr => sr.ServiceDate),
            "totalcost" => descending
                ? query.OrderByDescending(sr => sr.TotalCost)
                : query.OrderBy(sr => sr.TotalCost),
            "mileage" => descending
                ? query.OrderByDescending(sr => sr.Mileage)
                : query.OrderBy(sr => sr.Mileage),
            "title" => descending
                ? query.OrderByDescending(sr => sr.Title)
                : query.OrderBy(sr => sr.Title),
            _ => query.OrderByDescending(sr => sr.ServiceDate)
        };
    }
}

