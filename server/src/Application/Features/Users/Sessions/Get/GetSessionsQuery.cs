using Application.Abstractions.Messaging;

namespace Application.Features.Users.Sessions.Get;

public record GetSessionsQuery : IQuery<SessionsDto>;