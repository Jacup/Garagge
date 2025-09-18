using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.VehicleEnergyTypes.Delete;
using MediatR;

namespace Api.Endpoints.VehicleEnergyType;

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
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.VehicleEnergyTypes);
    }
}
