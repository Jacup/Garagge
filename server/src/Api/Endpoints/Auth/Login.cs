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
        app.MapPost("auth/login", async (AuthLoginRequest request, ISender sender, HttpContext httpContext, IConfiguration configuration, CancellationToken cancellationToken) =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext.Request.Headers.UserAgent.ToString();
                
                var command = new LoginUserCommand(request.Email, request.Password, request.RememberMe, ipAddress, userAgent);

                Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return CustomResults.Problem(result);
                
                DateTimeOffset? cookieExpires = request.RememberMe 
                    ? result.Value.RefreshTokenExpiresAt 
                    : null;
                
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = configuration.GetValue<bool>("Security:UseSecureCookies"),
                    SameSite = SameSiteMode.Strict,
                    Expires = cookieExpires
                };

                httpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);

                return Results.Ok(Result.Success(new AuthLoginResponse(result.Value.AccessToken)));
            })
            .AllowAnonymous()
            .Produces<AuthLoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tags.Auth);
    }
}

internal sealed record AuthLoginRequest(string Email, string Password, bool RememberMe);

internal sealed record AuthLoginResponse(string AccessToken);