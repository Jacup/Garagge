using Api.Infrastructure;
using Application.Users.GetById;
using MediatR;
using Api.Extensions;
using Application.Core;
using Application.Users;
using Infrastructure.Authentication;
using System.Security.Claims;

namespace Api.Endpoints.Users;

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