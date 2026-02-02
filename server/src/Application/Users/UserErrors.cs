using Application.Core;

namespace Application.Users;

public static class UserErrors
{
    public static readonly Error FirstNameRequired = Error.Validation(
        "User.FirstNameRequired",
        "First name is required.");

    public static readonly Error LastNameRequired = Error.Validation(
        "User.LastNameRequired",
        "Last name is required.");

    public static readonly Error EmailRequired = Error.Validation(
        "User.EmailRequired",
        "Email is required");

    public static readonly Error EmailInvalid = Error.Problem(
        "User.EmailInvalid",
        "Email is not valid");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "User.EmailNotUnique",
        "This email is already in use.");

    public static readonly Error PasswordRequired = Error.Validation(
        "User.PasswordRequired",
        "Password is required.");

    public static Error PasswordTooShort(int minPasswordLength) => Error.Validation(
        "User.PasswordTooShort",
        $"Password must be at least {minPasswordLength} characters long.");

    public static readonly Error PasswordSameAsOld = Error.Validation(
        "User.PasswordSameAsOld",
        "New password must be different from current password.");

    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        "User was not found.");

    // the below errors should be reviewed
    public static readonly Error UpdateFailed = Error.Problem(
        "User.UpdateFailed",
        "Update user failed");

    public static Error Unauthorized => Error.Unauthorized(
        "User.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error DeleteCurrentSession = Error.Problem(
        "User.SessionsDelete",
        "Cannot delete current session.");

    public static readonly Error DeleteSessionFailed = Error.Failure(
        "User.SessionsDeleteFailed",
        "Delete session failed");

    public static readonly Error SessionNotFound = Error.NotFound(
        "User.SessionNotFound",
        "Session not found");
}