using Application.Abstractions.Messaging;

namespace Application.Features.Auth.Login;

public sealed record LoginUserCommand(
    string Email,
    string Password,
    bool RememberMe,
    string? IpAddress,
    string? UserAgent)
    : ICommand<LoginUserResponse>;