using Application.Core;

namespace Application.Auth;

public static class AuthErrors
{
    public static readonly Error MissingFirstName = Error.Problem(
        "Auth.MissingFirstName",
        "First name is required.");

    public static readonly Error MissingLastName = Error.Problem(
        "Auth.MissingLastName",
        "Last name is required.");

    public static readonly Error MissingEmail = Error.Problem(
        "Auth.MissingEmail",
        "Email is required");
    
    public static readonly Error MissingPassword = Error.Problem(
        "Auth.MissingPassword",
        "Password is required");

    public static readonly Error InvalidEmail = Error.Problem(
        "Auth.InvalidEmail",
        "Email is not valid");
    
    public static readonly Error NewPasswordSameAsOld = Error.Problem(
        "Auth.NewPasswordSameAsOld",
        "New password must be different from the old password.");
    
    public static Error InvalidPassword(int minPasswordLength) => Error.Problem(
        "Auth.InvalidPassword",
        $"Password must be at least {minPasswordLength} characters long.");
    
    public static readonly Error WrongEmailOrPassword = Error.Unauthorized(
        "Auth.WrongEmailOrPassword",
        "The provided email or password is incorrect");

    public static readonly Error WrongPassword = Error.Problem(
        "Auth.WrongPassword",
        "The provided password is incorrect");
    
    public static readonly Error CreateFailed = Error.Failure(
        "Auth.CreateFailed",
        "Create user failed");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Auth.EmailNotUnique",
        "The provided email is not unique");
}