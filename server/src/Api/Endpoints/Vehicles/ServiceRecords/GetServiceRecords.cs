using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Features.ServiceRecords;
using Application.Features.ServiceRecords.Get;
using Domain.Enums.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords;

internal sealed class GetServiceRecords : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/{vehicleId:guid}/service-records", async (
                [FromRoute] Guid vehicleId,
                [FromServices] ISender sender,
                CancellationToken cancellationToken,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? searchTerm = null,
                [FromQuery] ServiceRecordType? type = null,
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
                    type,
                    dateFrom,
                    dateTo,
                    sortBy,
                    sortDescending);

                Result<PagedList<ServiceRecordDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<PagedList<ServiceRecordDto>>()
            .WithTags(Tags.ServiceRecords);
    }
}