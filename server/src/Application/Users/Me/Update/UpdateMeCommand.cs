using Application.Abstractions.Messaging;

namespace Application.Users.Me.Update;

public sealed record UpdateMeCommand(string Email, string FirstName, string LastName) : ICommand<UserDto>;