using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Authentication;

public static class AuthCookieNames
{
    public const string AccessToken = "accessToken";
    public const string RefreshToken = "refreshToken";
}

public static class AuthCookiePaths
{
    public const string AuthRoot = "/api/auth";
    public const string Root = "/";
}

public static class AuthCookieFactory
{
    public static CookieOptions GetDefaultOptions(IConfiguration config, DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = config.GetValue<bool>("Security:UseSecureCookies"),
        SameSite = SameSiteMode.Strict,
        Expires = expires,
        Path = AuthCookiePaths.Root
    };

    public static CookieOptions GetRefreshTokenOptions(IConfiguration config, DateTimeOffset expires)
    {
        var options = GetDefaultOptions(config, expires);
        options.Path = AuthCookiePaths.AuthRoot;
        return options;
    }

    public static CookieOptions GetDeleteOptions(string path = AuthCookiePaths.Root) => new() { HttpOnly = true, Path = path };
}