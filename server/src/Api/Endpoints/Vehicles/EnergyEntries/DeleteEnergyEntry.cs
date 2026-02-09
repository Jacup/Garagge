using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Features.EnergyEntries.Delete;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

internal sealed class DeleteEnergyEntry : IEndpoint
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
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.EnergyEntries);
    }
}