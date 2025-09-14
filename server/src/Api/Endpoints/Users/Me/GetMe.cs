using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Users;
using Application.Users.GetById;
using Infrastructure.Authentication;
using MediatR;
using System.Security.Claims;

namespace Api.Endpoints.Users.Me;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetUserByIdQuery(user.GetUserId());
                
                Result<UserDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .HasPermission(Permissions.UsersAccess)
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.Users);
    }
}