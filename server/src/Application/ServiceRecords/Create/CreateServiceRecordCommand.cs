using Application.Abstractions.Messaging;
using Application.ServiceItems.Create;

namespace Application.ServiceRecords.Create;

public sealed record CreateServiceRecordCommand(
    string Title,
    DateTime ServiceDate,
    Guid ServiceTypeId,
    Guid VehicleId,
    string? Notes,
    int? Mileage,
    decimal? ManualCost,
    ICollection<CreateServiceItemCommand> ServiceItems)
    : ICommand<ServiceRecordDto>;