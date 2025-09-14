using Application.Core;

namespace Application.Users;

public static class UserErrors
{
    public static readonly Error MissingFirstName = Error.Problem(
        "User.MissingFirstName",
        "First name is required.");

    public static readonly Error MissingLastName = Error.Problem(
        "User.MissingLastName",
        "Last name is required.");

    public static readonly Error MissingEmail = Error.Problem(
        "User.MissingEmail",
        "Email is required");
    
    public static readonly Error InvalidEmail = Error.Problem(
        "User.InvalidEmail",
        "Email is not valid");
    
    public static readonly Error UpdateFailed = Error.Problem(
        "User.UpdateFailed",
        "Update user failed");
    
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized => Error.Unauthorized(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error WrongPassword = Error.Unauthorized(
        "Users.WrongPassword",
        "The provided password is incorrect");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");
}