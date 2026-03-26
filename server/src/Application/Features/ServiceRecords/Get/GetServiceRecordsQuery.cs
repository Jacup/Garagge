using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Enums.Services;

namespace Application.Features.ServiceRecords.Get;

public sealed record GetServiceRecordsQuery(
    Guid VehicleId,
    int Page,
    int PageSize,
    string? SearchTerm = null,
    ServiceRecordType? Type = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SortBy = null,
    bool SortDescending = false)
    : IQuery<PagedList<ServiceRecordDto>>;