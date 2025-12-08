using Api.Extensions;
using Api.Infrastructure;
using Application.Users.Sessions;
using Application.Users.Sessions.Get;
using MediatR;

namespace Api.Endpoints.Users.Me.Sessions;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me/sessions", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetSessionsQuery(), cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<SessionsDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(Tags.Users);
    }
}