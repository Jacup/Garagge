using Application.Abstractions.Messaging;

namespace Application.Users.Me.Get;

public sealed record GetMeQuery : IQuery<UserDto>;
