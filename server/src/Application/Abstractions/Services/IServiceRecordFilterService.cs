﻿using Application.ServiceRecords.Get;
using Domain.Entities.Services;

namespace Application.Abstractions.Services;

public interface IServiceRecordFilterService
{
    IQueryable<ServiceRecord> ApplyFilters(IQueryable<ServiceRecord> query, GetServiceRecordsQuery request);
    IQueryable<ServiceRecord> ApplySorting(IQueryable<ServiceRecord> query, string? sortBy, bool descending);
    bool RequiresInMemorySorting(string? sortBy);
}

