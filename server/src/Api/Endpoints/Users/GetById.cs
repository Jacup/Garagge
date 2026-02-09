using Api.Infrastructure;
using MediatR;
using Api.Extensions;
using Application.Core;
using Application.Features.Users;
using Application.Features.Users.GetById;

namespace Api.Endpoints.Users;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}", async (Guid userId, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetUserByIdQuery(userId);

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
