using Application.Abstractions.Messaging;
using Application.Features.ServiceItems.Create;
using Domain.Enums.Services;

namespace Application.Features.ServiceRecords.Create;

public sealed record CreateServiceRecordCommand(
    string Title,
    DateTime ServiceDate,
    ServiceRecordType Type,
    Guid VehicleId,
    string? Notes,
    int? Mileage,
    decimal? ManualCost,
    ICollection<CreateServiceItemCommand> ServiceItems)
    : ICommand<ServiceRecordDto>;