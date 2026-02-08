using Application.Features.Auth.Logout;
using Infrastructure.Authentication;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (ISender sender, HttpContext httpContext, CancellationToken cancellationToken) =>
            {
                if (httpContext.Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    var command = new LogoutUserCommand(refreshToken);
                    await sender.Send(command, cancellationToken);
                }

                httpContext.Response.Cookies.Delete(AuthCookieNames.AccessToken, AuthCookieFactory.GetDeleteOptions());
                httpContext.Response.Cookies.Delete(AuthCookieNames.RefreshToken, AuthCookieFactory.GetDeleteOptions(AuthCookiePaths.AuthRoot));

                return Results.NoContent();
            })
            .AllowAnonymous()
            .WithTags(Tags.Auth);
    }
}