using Application.Abstractions.Messaging;
using Application.Features.ServiceRecords;

namespace Application.Features.ServiceTypes.Update;

public sealed record UpdateServiceTypeCommand(Guid Id, string Name) : ICommand<ServiceTypeDto>;
