using Application.Abstractions.Messaging;
using Application.ServiceItems;
using Application.ServiceItems.Create;
using Domain.Entities.Services;

namespace Application.ServiceRecords.Create;

public sealed record CreateServiceRecordCommand(
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    Guid ServiceTypeId,
    Guid VehicleId,
    ICollection<CreateServiceItemCommand> ServiceItems)
    : ICommand<ServiceRecordDto>;