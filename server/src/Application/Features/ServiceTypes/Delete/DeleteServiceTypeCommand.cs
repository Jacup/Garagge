using Application.Abstractions.Messaging;

namespace Application.Features.ServiceTypes.Delete;

public sealed record DeleteServiceTypeCommand(Guid Id) : ICommand;
