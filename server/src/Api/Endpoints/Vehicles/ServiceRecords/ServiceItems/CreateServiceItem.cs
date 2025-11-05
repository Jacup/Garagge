using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.ServiceItems;
using Application.ServiceItems.Create;
using Domain.Enums.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceItems;

internal sealed class CreateServiceItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/{vehicleId:guid}/service-records/{serviceRecordId:guid}/service-items",
                async (
                    [FromRoute] Guid vehicleId,
                    [FromRoute] Guid serviceRecordId,
                    [FromBody] ServiceItemCreateRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new CreateServiceItemCommand(
                        serviceRecordId,
                        request.Name,
                        request.Type,
                        request.UnitPrice,
                        request.Quantity,
                        request.PartNumber,
                        request.Notes);

                    Result<ServiceItemDto> result = await sender.Send(query, cancellationToken);

                    return result.Match(
                        serviceItem => Results.Created($"/vehicles/{vehicleId}/service-records/{serviceRecordId}/service-items/{serviceItem.Id}", serviceItem), 
                        CustomResults.Problem
                    );
                })
            .RequireAuthorization()
            .Produces<ServiceItemDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}

internal sealed record ServiceItemCreateRequest(
    string Name,
    ServiceItemType Type, 
    decimal UnitPrice, 
    decimal Quantity, 
    string? PartNumber,
    string? Notes);
