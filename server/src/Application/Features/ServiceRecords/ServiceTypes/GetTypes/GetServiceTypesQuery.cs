using Application.Abstractions.Messaging;

namespace Application.Features.ServiceRecords.ServiceTypes.GetTypes;

public sealed record GetServiceTypesQuery : IQuery<ICollection<ServiceTypeDto>>;