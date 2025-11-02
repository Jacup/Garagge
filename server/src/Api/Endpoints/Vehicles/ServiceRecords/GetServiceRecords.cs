using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.ServiceRecords;
using Application.ServiceRecords.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords;

internal sealed class GetServiceRecords : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/{vehicleId:guid}/service-records", async (
                [FromRoute] Guid vehicleId,
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? searchTerm = null,
                [FromQuery] Guid? serviceTypeId = null,
                [FromQuery] DateTime? dateFrom = null,
                [FromQuery] DateTime? dateTo = null,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false) =>
            {
                var query = new GetServiceRecordsQuery(
                    vehicleId, 
                    page, 
                    pageSize,
                    searchTerm,
                    serviceTypeId,
                    dateFrom,
                    dateTo,
                    sortBy,
                    sortDescending);

                Result<PagedList<ServiceRecordDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<PagedList<ServiceRecordDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}