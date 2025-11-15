using Api.Endpoints.Vehicles.ServiceRecords.ServiceItems;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.ServiceItems.Create;
using Application.ServiceRecords;
using Application.ServiceRecords.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords;

internal sealed class CreateServiceRecord : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/{vehicleId:guid}/service-records",
                async (
                    [FromRoute] Guid vehicleId,
                    [FromBody] ServiceRecordCreateRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new CreateServiceRecordCommand(
                        request.Title,
                        request.ServiceDate,
                        request.ServiceTypeId,
                        vehicleId,
                        request.Notes,
                        request.Mileage,
                        request.ManualCost,
                        request.ServiceItems.Select(serviceItemRequest => new CreateServiceItemCommand(
                            Guid.Empty,
                            serviceItemRequest.Name,
                            serviceItemRequest.Type,
                            serviceItemRequest.UnitPrice,
                            serviceItemRequest.Quantity,
                            serviceItemRequest.PartNumber,
                            serviceItemRequest.Notes)).ToList()
                        );

                    Result<ServiceRecordDto> result = await sender.Send(query, cancellationToken);

                    return result.Match(
                        dto => Results.Created($"/api/vehicles/{vehicleId}/service-records/{dto.Id}", dto),
                        CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<ServiceRecordDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.ServiceRecords);
    }
}

internal sealed record ServiceRecordCreateRequest(
    string Title,
    DateTime ServiceDate,
    Guid ServiceTypeId,
    string? Notes,
    int? Mileage,
    decimal? ManualCost,
    ICollection<ServiceItemCreateRequest> ServiceItems);