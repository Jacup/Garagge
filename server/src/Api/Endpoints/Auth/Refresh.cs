using Api.Infrastructure;
using Application.Core;
using Application.Features.Auth;
using Application.Features.Auth.Refresh;
using Infrastructure.Authentication;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (ISender sender, HttpContext httpContext, IConfiguration configuration, CancellationToken cancellationToken) =>
            {
                if (!httpContext.Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken))
                {
                    return CustomResults.Problem(Result.Failure(AuthErrors.TokenInvalid));
                }

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                var command = new RefreshTokenCommand(refreshToken, ipAddress, userAgent);

                Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                {
                    httpContext.Response.Cookies.Delete(AuthCookieNames.AccessToken, AuthCookieFactory.GetDeleteOptions());
                    httpContext.Response.Cookies.Delete(AuthCookieNames.RefreshToken, AuthCookieFactory.GetDeleteOptions(AuthCookiePaths.AuthRoot));
                    
                    return CustomResults.Problem(result);
                }

                httpContext.Response.Cookies.Append(AuthCookieNames.AccessToken, result.Value.AccessToken, AuthCookieFactory.GetDefaultOptions(configuration));
                httpContext.Response.Cookies.Append(AuthCookieNames.RefreshToken, result.Value.RefreshToken, AuthCookieFactory.GetRefreshTokenOptions(configuration, result.Value.RefreshTokenExpiresAt));
                
                return Results.NoContent();
            })
            .AllowAnonymous()
            .WithTags(Tags.Auth);
    }
}