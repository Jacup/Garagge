using Application.Abstractions.Messaging;
using Application.Features.ServiceRecords;

namespace Application.Features.ServiceTypes.GetAll;

public sealed record GetServiceTypesQuery : IQuery<ICollection<ServiceTypeDto>>;