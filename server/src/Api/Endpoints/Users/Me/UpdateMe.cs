using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Users;
using Application.Users.Update;
using Infrastructure.Authentication;
using MediatR;
using System.Security.Claims;

namespace Api.Endpoints.Users.Me;

internal sealed class UpdateMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/me", async (UpdateMeRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var userId = user.GetUserId();
                var command = new UpdateUserCommand(userId, request.Email, request.FirstName, request.LastName);
                
                Result<UserDto> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .HasPermission(Permissions.UsersAccess)
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.Users);
    }
}

internal sealed record UpdateMeRequest(string FirstName, string LastName, string Email);