using Application.Abstractions.Messaging;

namespace Application.Auth.Logout;

public sealed record LogoutUserCommand(string RefreshToken) : ICommand;