using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Users;
using Application.Users.Me.Update;
using MediatR;

namespace Api.Endpoints.Users.Me;

internal sealed class UpdateMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/me", async (UserUpdateMeRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new UpdateMeCommand(request.Email, request.FirstName, request.LastName);
                
                Result<UserDto> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<UserDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.Users);
    }
}

internal sealed record UserUpdateMeRequest(string Email, string FirstName, string LastName);