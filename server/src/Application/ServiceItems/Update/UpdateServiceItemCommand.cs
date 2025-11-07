using Application.Abstractions.Messaging;
using Domain.Enums.Services;

namespace Application.ServiceItems.Update;

public record UpdateServiceItemCommand(
    Guid ServiceItemId,
    Guid ServiceRecordId,
    string Name,
    ServiceItemType Type,
    decimal UnitPrice,
    decimal Quantity,
    string? PartNumber,
    string? Notes)
    : ICommand<ServiceItemDto>;