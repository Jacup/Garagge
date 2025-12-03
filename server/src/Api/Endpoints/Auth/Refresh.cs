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
                    return Results.Unauthorized();
                }
                
                string? ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                string? userAgent = httpContext.Request.Headers.UserAgent.ToString();

                var command = new RefreshTokenCommand(refreshToken, ipAddress, userAgent);

                Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                {
                    httpContext.Response.Cookies.Delete("refreshToken");
                    return CustomResults.Problem(result);
                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = configuration.GetValue<bool>("Security:UseSecureCookies"),
                    SameSite = SameSiteMode.Strict,
                    Expires = result.Value.RefreshTokenExpiresAt
                };

                httpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);

                return Results.Ok(new AuthLoginResponse(result.Value.AccessToken));
            })
            .AllowAnonymous()
            .WithTags(Tags.Auth);
    }
}