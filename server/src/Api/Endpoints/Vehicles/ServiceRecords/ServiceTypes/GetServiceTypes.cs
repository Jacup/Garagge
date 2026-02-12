using Api.Extensions;
using Api.Infrastructure;
using Application.Features.ServiceRecords;
using Application.Features.ServiceTypes.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

internal sealed class GetServiceRecordTypes : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/service-records/types", async (
                [FromServices] ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetServiceTypesQuery();

                var result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<ICollection<ServiceTypeDto>>()
            .WithTags(Tags.ServiceTypes);
    }
}