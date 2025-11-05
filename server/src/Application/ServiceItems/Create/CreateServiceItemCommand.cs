using Application.Abstractions.Messaging;
using Domain.Enums.Services;

namespace Application.ServiceItems.Create;

public sealed record CreateServiceItemCommand(
    Guid ServiceRecordId,
    string Name,
    ServiceItemType Type,
    decimal UnitPrice,
    decimal Quantity,
    string? PartNumber,
    string? Notes)
    : ICommand<ServiceItemDto>;