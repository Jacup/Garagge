using Application.Abstractions.Messaging;

namespace Application.Features.ServiceRecords.Delete;

public sealed record DeleteServiceRecordCommand(Guid ServiceRecordId, Guid VehicleId) : ICommand;