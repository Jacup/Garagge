using Api.Infrastructure;
using Application.Auth;
using Application.Auth.Login;
using Application.Core;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login",
                async (LoginRequest request, ISender sender, HttpContext httpContext, IConfiguration configuration, CancellationToken cancellationToken) =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    var userAgent = httpContext.Request.Headers.UserAgent.ToString();

                    var command = new LoginUserCommand(request.Email, request.Password, request.RememberMe, ipAddress, userAgent);

                    Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

                    if (result.IsFailure)
                        return CustomResults.Problem(result);
                    
                    var baseCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = configuration.GetValue<bool>("Security:UseSecureCookies"),
                        SameSite = SameSiteMode.Strict,
                    };

                    httpContext.Response.Cookies.Append("accessToken", result.Value.AccessToken, new CookieOptions(baseCookieOptions) { Path = "/", Expires = null});
                    httpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, new CookieOptions(baseCookieOptions)
                    {
                        Path = "/api/auth/",
                        Expires = request.RememberMe ? result.Value.RefreshTokenExpiresAt : null
                    });

                    return Results.NoContent();
                })
            .AllowAnonymous()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tags.Auth);
    }
}

internal sealed record LoginRequest(string Email, string Password, bool RememberMe);