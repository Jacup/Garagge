using Api.Extensions;
using Api.Infrastructure;
using Application.Users.Sessions.DeleteAll;
using MediatR;

namespace Api.Endpoints.Users.Me.Sessions;

internal sealed class DeleteAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/me/sessions", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new DeleteSessionsCommand();
                var result = await sender.Send(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tags.Users);
    }
}