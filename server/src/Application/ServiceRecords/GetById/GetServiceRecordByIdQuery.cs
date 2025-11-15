using Application.Abstractions.Messaging;

namespace Application.ServiceRecords.GetById;

public sealed record GetServiceRecordByIdQuery(Guid VehicleId, Guid ServiceRecordId) : IQuery<ServiceRecordDto>;