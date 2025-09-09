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

internal sealed class GetMyVehicles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/my", async (
                string? searchTerm,
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken,
                int pageSize = 10, 
                int page = 1) =>
            {
                var query = new GetMyVehiclesQuery(user.GetUserId(), page, pageSize, searchTerm);

                Result<PagedList<VehicleDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .Produces<PagedList<VehicleDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Vehicles);
    }
}