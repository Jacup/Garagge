using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Users.Me.Delete;
using MediatR;

namespace Api.Endpoints.Users.Me;

public class DeleteMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/me", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new DeleteMeCommand();

                Result result = await sender.Send(query, cancellationToken);

                return result.Match(() => Results.Ok(), CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.Users);
    }
}