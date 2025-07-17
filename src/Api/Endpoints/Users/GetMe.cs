using Api.Infrastructure;
using Application.Users.GetById;
using MediatR;
using SharedKernel;
using Api.Extensions;
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
                
                Result<UserResponse> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Users);
    }
}