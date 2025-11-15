using Application.Abstractions.Messaging;

namespace Application.ServiceRecords.Update;

public record UpdateServiceRecordCommand(
    Guid ServiceRecordId,
    string Title,
    DateTime ServiceDate,
    Guid ServiceTypeId,
    Guid VehicleId,
    string? Notes = null,
    int? Mileage = null,
    decimal? ManualCost = null)
    : ICommand<ServiceRecordDto>;
