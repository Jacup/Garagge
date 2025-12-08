using Api.Extensions;
using Api.Infrastructure;
using Application.Users.Sessions.Delete;
using MediatR;

namespace Api.Endpoints.Users.Me.Sessions;

internal sealed class DeleteById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/me/sessions/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new DeleteSessionCommand(id);
                var result = await sender.Send(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.Users);
    }
}