﻿namespace ApiIntegrationTests.Contracts.V1;

using Domain.Enums.Services;

internal sealed record ServiceRecordCreateRequest(
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    ServiceRecordType Type,
    ICollection<ServiceItemCreateRequest> ServiceItems);
    
internal sealed record ServiceRecordUpdateRequest(
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    ServiceRecordType Type);
