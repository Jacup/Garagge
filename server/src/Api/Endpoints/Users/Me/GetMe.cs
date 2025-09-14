using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Users;
using Application.Users.Me.Get;
using MediatR;

namespace Api.Endpoints.Users.Me;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetMeQuery();
                
                Result<UserDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.Users);
    }
}