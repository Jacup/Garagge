using Application.Abstractions.Messaging;

namespace Application.Users.Sessions.Get;

public record GetSessionsQuery : IQuery<SessionsDto>;