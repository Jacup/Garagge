using Application.Abstractions.Messaging;

namespace Application.Features.Users.Me.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, bool LogoutAllDevices) : ICommand;