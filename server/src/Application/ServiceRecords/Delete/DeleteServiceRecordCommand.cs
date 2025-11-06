using Application.Abstractions.Messaging;

namespace Application.ServiceRecords.Delete;

public sealed record DeleteServiceRecordCommand(Guid ServiceRecordId, Guid VehicleId) : ICommand;