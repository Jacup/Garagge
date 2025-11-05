using Domain.Enums.Services;

namespace Application.ServiceItems;

public record ServiceItemDto(
    Guid Id,
    string Name,
    ServiceItemType Type,
    decimal UnitPrice,
    decimal Quantity,
    decimal TotalPrice,
    string? PartNumber,
    string? Notes,
    Guid ServiceRecordId,
    DateTime CreatedDate,
    DateTime UpdatedDate);