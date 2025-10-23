using Api.Extensions;
using Api.Infrastructure;
using Application.Auth.Login;
using Application.Core;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new LoginUserCommand(request.Email, request.Password);

                Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .AllowAnonymous()
            .Produces<LoginUserResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tags.Auth);
    }
}

internal sealed record LoginRequest(string Email, string Password);