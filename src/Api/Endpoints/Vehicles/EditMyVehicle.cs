using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.EditMyVehicle;
using Domain.Enums;
using MediatR;
using System.Security.Claims;

namespace Api.Endpoints.Vehicles;

public class EditMyVehicle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "vehicles/my/edit/{id:guid}",
                async (Guid id, EditMyVehicleRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new EditMyVehicleCommand
                    (
                        id,
                        request.Brand,
                        request.Model,
                        request.PowerType,
                        request.ManufacturedYear,
                        request.Type,
                        request.VIN
                    );

                    Result<VehicleDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Vehicles);
    }
}

record EditMyVehicleRequest(
    string Brand,
    string Model,
    PowerType PowerType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null);