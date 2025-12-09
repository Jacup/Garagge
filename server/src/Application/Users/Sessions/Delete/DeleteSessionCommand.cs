using Application.Abstractions.Messaging;

namespace Application.Users.Sessions.Delete;

public sealed record DeleteSessionCommand(Guid SessionId) : ICommand;