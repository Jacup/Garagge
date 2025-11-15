﻿using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.ServiceRecords;
using Application.ServiceRecords.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords;

internal sealed class UpdateServiceRecord : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "vehicles/{vehicleId:guid}/service-records/{serviceRecordId:guid}",
                async (
                    [FromRoute] Guid vehicleId,
                    [FromRoute] Guid serviceRecordId,
                    [FromBody] ServiceRecordUpdateRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateServiceRecordCommand(
                        serviceRecordId,
                        request.Title,
                        request.ServiceDate,
                        request.ServiceTypeId,
                        vehicleId,
                        request.Notes,
                        request.Mileage,
                        request.ManualCost);

                    Result<ServiceRecordDto> result = await sender.Send(command, cancellationToken);

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

internal sealed record ServiceRecordUpdateRequest(
    string Title,
    DateTime ServiceDate,
    Guid ServiceTypeId,
    string? Notes,
    int? Mileage,
    decimal? ManualCost);