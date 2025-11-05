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
                        request.Notes,
                        request.Mileage,
                        request.ServiceDate,
                        request.ManualCost,
                        request.ServiceTypeId,
                        vehicleId,
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

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<ServiceRecordDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}

internal sealed record ServiceRecordCreateRequest(
    string Title,
    string? Notes,
    int? Mileage,
    DateTime ServiceDate,
    decimal? ManualCost,
    Guid ServiceTypeId,
    ICollection<ServiceItemCreateRequest> ServiceItems);