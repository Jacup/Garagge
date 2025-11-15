using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.ServiceRecords;
using Application.ServiceRecords.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords;

internal sealed class GetServiceRecordById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/{vehicleId:guid}/service-records/{serviceRecordId:guid}", async (
                [FromRoute] Guid vehicleId,
                [FromRoute] Guid serviceRecordId,
                [FromServices] ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetServiceRecordByIdQuery(vehicleId, serviceRecordId);

                Result<ServiceRecordDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<ServiceRecordDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.ServiceRecords);
    }
}