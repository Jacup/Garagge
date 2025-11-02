using Domain.Entities.Services;

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
    Guid VehicleId,
    DateTime CreatedDate,
    DateTime UpdatedDate
);