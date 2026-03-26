using Application.Abstractions.Messaging;
using Domain.Enums.Services;

namespace Application.Features.ServiceRecords.Update;

public record UpdateServiceRecordCommand(
    Guid ServiceRecordId,
    string Title,
    DateTime ServiceDate,
    ServiceRecordType Type,
    Guid VehicleId,
    string? Notes = null,
    int? Mileage = null,
    decimal? ManualCost = null)
    : ICommand<ServiceRecordDto>;
