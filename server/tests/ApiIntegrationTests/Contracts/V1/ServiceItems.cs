using Domain.Enums.Services;

namespace ApiIntegrationTests.Contracts.V1;

internal sealed record ServiceItemCreateRequest(
    string Name,
    ServiceItemType Type, 
    decimal UnitPrice, 
    decimal Quantity, 
    string? PartNumber,
    string? Notes);