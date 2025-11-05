using Application.ServiceItems;

namespace Application.ServiceRecords;

public record ServiceRecordDto(
    Guid Id,
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal TotalCost,
    Guid TypeId,
    string Type,
    ICollection<ServiceItemDto> ServiceItems,
    Guid VehicleId,
    DateTime CreatedDate,
    DateTime UpdatedDate
);