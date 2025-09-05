using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries.DeleteEnergyEntry;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

public class DeleteEnergyEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "vehicles/{vehicleId:guid}/energy-entries/{id:guid}",
                async (Guid id, Guid vehicleId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteEnergyEntryCommand(id, vehicleId);

                    Result result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.NoContent, CustomResults.Problem);
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.EnergyEntries);
    }
}