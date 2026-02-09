using Application.Abstractions.Messaging;

namespace Application.Features.Users.Sessions.DeleteById;

public sealed record DeleteSessionCommand(Guid SessionId) : ICommand;