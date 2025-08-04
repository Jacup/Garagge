using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.CreateMyVehicle;
using MediatR;

namespace Api.Endpoints.Vehicles;

public class CreateVehicle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("vehicles/my", async (CreateMyVehicleCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            Result<VehicleDto> result = await sender.Send(command, cancellationToken);

            return result.Match(
                vehicle => Results.Created($"/vehicles/{vehicle.Id}", vehicle),
                CustomResults.Problem
            );
        })
        .Produces<VehicleDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .HasPermission(Permissions.UsersAccess)
        .WithTags(Tags.Vehicles);
    }
}