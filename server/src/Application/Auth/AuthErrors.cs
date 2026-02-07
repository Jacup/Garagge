using Application.Core;

namespace Application.Auth;

public static class AuthErrors
{
    public static readonly Error CredentialsInvalid = Error.Unauthorized(
        "Auth.CredentialsInvalid",
        "Invalid email or password.");
    
    public static readonly Error PasswordInvalid = Error.Validation(
        "Auth.PasswordInvalid",
        "Password is invalid.");

    public static readonly Error TokenInvalid = Error.Unauthorized(
        "Auth.TokenInvalid",
        "Token is invalid.");

    public static readonly Error TokenRevoked = Error.Unauthorized(
        "Auth.TokenRevoked",
        "Token has been revoked.");

    public static readonly Error TokenExpired = Error.Unauthorized(
        "Auth.TokenExpired",
        "Token has expired.");
}