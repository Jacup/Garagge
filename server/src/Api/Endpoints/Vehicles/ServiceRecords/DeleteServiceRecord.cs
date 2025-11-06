using Api.Extensions;
using Api.Infrastructure;
using Application.ServiceRecords.Delete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords;

internal sealed class DeleteServiceRecord : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "vehicles/{vehicleId:guid}/service-records/{serviceRecordId:guid}",
                async (
                    [FromRoute] Guid vehicleId,
                    [FromRoute] Guid serviceRecordId,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeleteServiceRecordCommand(serviceRecordId, vehicleId);

                    var result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.NoContent, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.ServiceRecords);
    }
}