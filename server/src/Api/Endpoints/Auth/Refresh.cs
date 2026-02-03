using Api.Infrastructure;
using Application.Auth;
using Application.Auth.Refresh;
using Application.Core;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (ISender sender, HttpContext httpContext, IConfiguration configuration, CancellationToken cancellationToken) =>
            {
                if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                {
                    return CustomResults.Problem(Result.Failure(AuthErrors.TokenInvalid));
                }

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                var command = new RefreshTokenCommand(refreshToken, ipAddress, userAgent);

                Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                {
                    httpContext.Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
                    httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth/" });
                    return CustomResults.Problem(result);
                }

                var baseCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = configuration.GetValue<bool>("Security:UseSecureCookies"),
                    SameSite = SameSiteMode.Strict,
                    Expires = result.Value.RefreshTokenExpiresAt
                };

                httpContext.Response.Cookies.Append("accessToken", result.Value.AccessToken, new CookieOptions(baseCookieOptions) { Path = "/" });
                httpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, new CookieOptions(baseCookieOptions) { Path = "/auth/" });

                return Results.NoContent();
            })
            .AllowAnonymous()
            .WithTags(Tags.Auth);
    }
}