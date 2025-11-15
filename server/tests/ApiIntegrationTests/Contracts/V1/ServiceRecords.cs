namespace ApiIntegrationTests.Contracts.V1;

internal sealed record ServiceRecordCreateRequest(
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    Guid ServiceTypeId,
    ICollection<ServiceItemCreateRequest> ServiceItems);
    
internal sealed record ServiceRecordUpdateRequest(
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    Guid ServiceTypeId);