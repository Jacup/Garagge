using Application.Core;

namespace Application.Auth;

public static class AuthErrors
{
    public static readonly Error MissingEmail = Error.Problem(
        "Auth.MissingEmail",
        "Email is required");

    public static readonly Error InvalidEmail = Error.Problem(
        "Auth.InvalidEmail",
        "Email is not valid");

    public static readonly Error WrongEmailOrPassword = Error.Unauthorized(
        "Auth.WrongEmailOrPassword",
        "The provided email or password is incorrect");

    public static readonly Error CreateFailed = Error.Failure(
        "Auth.CreateFailed",
        "Create user failed");
    
    public static readonly Error EmailNotUnique = Error.Conflict(
        "Auth.EmailNotUnique",
        "The provided email is not unique");
}