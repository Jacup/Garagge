using Application.Abstractions.Messaging;

namespace Application.Features.ServiceRecords.GetById;

public sealed record GetServiceRecordByIdQuery(Guid VehicleId, Guid ServiceRecordId) : IQuery<ServiceRecordDto>;