using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.ServiceItems;
using Application.ServiceItems.Update;
using Domain.Enums.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceItems;

internal sealed class UpdateServiceItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "vehicles/{vehicleId:guid}/service-records/{serviceRecordId:guid}/service-items/{serviceItemId:guid}",
                async (
                    [FromRoute] Guid vehicleId,
                    [FromRoute] Guid serviceRecordId,
                    [FromRoute] Guid serviceItemId,
                    [FromBody] ServiceItemUpdateRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateServiceItemCommand(
                        serviceItemId,
                        serviceRecordId,
                        request.Name,
                        request.Type,
                        request.UnitPrice,
                        request.Quantity,
                        request.PartNumber,
                        request.Notes);

                    Result<ServiceItemDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<ServiceItemDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.ServiceItems);
    }
}

internal sealed record ServiceItemUpdateRequest(
    string Name,
    ServiceItemType Type,
    decimal UnitPrice,
    decimal Quantity,
    string? PartNumber,
    string? Notes);

