using Application.Abstractions.Messaging;

namespace Application.Features.Users.Me.Get;

public sealed record GetMeQuery : IQuery<UserDto>;
