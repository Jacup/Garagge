using Application.Abstractions.Messaging;

namespace Application.ServiceRecords.GetTypes;

public sealed record GetServiceTypesQuery : IQuery<ICollection<ServiceTypeDto>>;