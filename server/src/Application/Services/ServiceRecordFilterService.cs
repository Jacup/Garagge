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
            var searchTerm = request.SearchTerm.ToLower().Trim();

            query = query.Where(sr =>
                sr.Title.ToLower().Contains(searchTerm) ||
                (sr.Notes != null && sr.Notes.ToLower().Contains(searchTerm)));
        }

        if (request.ServiceTypeId.HasValue)
        {
            query = query.Where(sr => sr.TypeId == request.ServiceTypeId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            var dateFromUtc = request.DateFrom.Value.Kind == DateTimeKind.Utc
                ? request.DateFrom.Value
                : DateTime.SpecifyKind(request.DateFrom.Value, DateTimeKind.Utc);

            query = query.Where(sr => sr.ServiceDate >= dateFromUtc);
        }

        if (request.DateTo.HasValue)
        {
            var dateFromUtc = request.DateTo.Value.Kind == DateTimeKind.Utc
                ? request.DateTo.Value
                : DateTime.SpecifyKind(request.DateTo.Value, DateTimeKind.Utc);

            query = query.Where(sr => sr.ServiceDate <= dateFromUtc);
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
            "totalcost" => query.OrderByDescending(sr => sr.ServiceDate), // TotalCost sorting handled in memory
            "mileage" => descending
                ? query.OrderByDescending(sr => sr.Mileage)
                : query.OrderBy(sr => sr.Mileage),
            "title" => descending
                ? query.OrderByDescending(sr => sr.Title)
                : query.OrderBy(sr => sr.Title),
            _ => query.OrderByDescending(sr => sr.ServiceDate)
        };
    }

    public bool RequiresInMemorySorting(string? sortBy)
    {
        return !string.IsNullOrWhiteSpace(sortBy) && sortBy.Equals("totalcost", StringComparison.OrdinalIgnoreCase);
    }
}