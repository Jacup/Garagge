using Application.Features.ServiceItems;
using Domain.Enums.Services;

namespace Application.Features.ServiceRecords;

public record ServiceRecordDto(
    Guid Id,
    string Title,
    ServiceRecordType Type,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal TotalCost,
    ICollection<ServiceItemDto> ServiceItems,
    Guid VehicleId,
    DateTime CreatedDate,
    DateTime UpdatedDate
);