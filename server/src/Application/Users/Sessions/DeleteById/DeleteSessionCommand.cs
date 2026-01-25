using Application.Abstractions.Messaging;

namespace Application.Users.Sessions.DeleteById;

public sealed record DeleteSessionCommand(Guid SessionId) : ICommand;