using Api.Infrastructure;
using Application.Users.Login;
using MediatR;
using Api.Extensions;
using Application.Core;

namespace Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login", async (LoginUserCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            Result<LoginUserResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .Produces<LoginUserResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(Tags.Users);
    }
}
