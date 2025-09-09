using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.VehicleEnergyTypes;
using Application.VehicleEnergyTypes.Delete;
using MediatR;

namespace Api.Endpoints.Vehicles.VehicleEnergyType;

internal sealed class DeleteVehicleEnergyType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "vehicles/{vehicleId:guid}/energy-types/{id:guid}",
                async (Guid vehicleId, Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteVehicleEnergyTypeCommand(id, vehicleId);

                    Result result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.NoContent, CustomResults.Problem);
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.VehicleEnergyTypes);
    }
}
