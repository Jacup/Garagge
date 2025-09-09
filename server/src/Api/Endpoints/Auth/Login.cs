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
        app.MapPost("auth/login", async (LoginUserCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .Produces<LoginUserResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags(Tags.Auth);
    }
}
