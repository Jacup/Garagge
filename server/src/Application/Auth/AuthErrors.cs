using Application.Core;

namespace Application.Auth;

public static class AuthErrors
{
    public static readonly Error WrongEmailOrPassword = Error.Unauthorized(
        "Auth.WrongEmailOrPassword",
        "The provided email or password is incorrect");
    
    public static readonly Error CreateFailed = Error.Failure(
        "Auth.CreateFailed",
        "Create user failed");
}