using Application.Abstractions.Messaging;

namespace Application.Features.ServiceItems.Delete;

public sealed record DeleteServiceItemCommand(Guid ServiceItemId, Guid ServiceRecordId, Guid VehicleId) : ICommand;
