using Application.Abstractions.Messaging;

namespace Application.Features.Auth.Logout;

public sealed record LogoutUserCommand(string RefreshToken) : ICommand;