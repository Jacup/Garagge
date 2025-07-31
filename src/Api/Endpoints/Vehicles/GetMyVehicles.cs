using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.GetMyVehicles;
using Infrastructure.Authentication;
using MediatR;
using System.Security.Claims;

namespace Api.Endpoints.Vehicles;

public class GetMyVehicles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/my", async (ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetMyVehiclesQuery(user.GetUserId());

                Result<ICollection<VehicleDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Vehicles);
    }
}