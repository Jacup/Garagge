using Application.Abstractions.Messaging;

namespace Application.ServiceRecords.Update;

public record UpdateServiceRecordCommand(
    Guid ServiceRecordId,
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    Guid ServiceTypeId,
    Guid VehicleId)
    : ICommand<ServiceRecordDto>;
