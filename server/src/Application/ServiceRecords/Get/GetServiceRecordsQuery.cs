using Application.Abstractions.Messaging;
using Application.Core;

namespace Application.ServiceRecords.Get;

public sealed record GetServiceRecordsQuery(
    Guid VehicleId,
    int Page,
    int PageSize,
    string? SearchTerm = null,
    Guid? ServiceTypeId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SortBy = null,
    bool SortDescending = false)
    : IQuery<PagedList<ServiceRecordDto>>;