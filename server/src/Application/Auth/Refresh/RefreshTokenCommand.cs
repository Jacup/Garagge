using Application.Abstractions.Messaging;

namespace Application.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress, string? UserAgent) : ICommand<LoginUserResponse>;