using Api.Extensions;
using Api.Infrastructure;
using Application.Auth.Logout;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (ISender sender, HttpContext httpContext, IConfiguration configuration, CancellationToken cancellationToken) =>
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = configuration.GetValue<bool>("Security:UseSecureCookies"), 
                    SameSite = SameSiteMode.Strict,
                };

                httpContext.Response.Cookies.Delete("accessToken", new CookieOptions(cookieOptions) { Path = "/" });
                httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions(cookieOptions) { Path = "/auth/" });

                if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
                    return Results.NoContent();

                var command = new LogoutUserCommand(refreshToken);
                var result = await sender.Send(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .AllowAnonymous()
            .WithTags(Tags.Auth);
    }
}