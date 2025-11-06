using Api.Extensions;
using Api.Infrastructure;
using Application.ServiceItems.Delete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceItems;

internal sealed class DeleteServiceItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "vehicles/{vehicleId:guid}/service-records/{serviceRecordId:guid}/service-items/{serviceItemId:guid}",
                async (
                    [FromRoute] Guid vehicleId,
                    [FromRoute] Guid serviceRecordId,
                    [FromRoute] Guid serviceItemId,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeleteServiceItemCommand(serviceItemId, serviceRecordId, vehicleId);

                    var result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.NoContent, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.ServiceItems);
    }
}