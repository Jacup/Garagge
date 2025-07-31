using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.GetMyVehicleById;
using Infrastructure.Authentication;
using MediatR;
using System.Security.Claims;

namespace Api.Endpoints.Vehicles;

public class GetMyVehicleById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/my/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetMyVehicleByIdQuery(user.GetUserId(), id);

                Result<VehicleDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })            
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Vehicles);
    }
}