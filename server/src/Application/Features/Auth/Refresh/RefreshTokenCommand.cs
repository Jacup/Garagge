using Application.Abstractions.Messaging;

namespace Application.Features.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress, string? UserAgent) : ICommand<LoginUserResponse>;