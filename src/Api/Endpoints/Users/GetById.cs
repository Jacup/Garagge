using Api.Infrastructure;
using Application.Users.GetById;
using MediatR;
using Api.Extensions;
using Application.Core;
using Application.Users;

namespace Api.Endpoints.Users;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId}", async (Guid userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetUserByIdQuery(userId);

            Result<UserResponse> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.UsersAccess)
        .Produces<UserDto>()
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags(Tags.Users);
    }
}
