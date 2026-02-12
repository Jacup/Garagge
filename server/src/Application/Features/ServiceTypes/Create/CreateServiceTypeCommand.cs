using Application.Abstractions.Messaging;
using Application.Features.ServiceRecords;

namespace Application.Features.ServiceTypes.Create;

public sealed record CreateServiceTypeCommand(string Name) : ICommand<ServiceTypeDto>;
