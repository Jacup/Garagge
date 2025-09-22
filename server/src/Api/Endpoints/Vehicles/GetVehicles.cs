using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.GetVehicles;
using MediatR;

namespace Api.Endpoints.Vehicles;

internal sealed class GetVehicles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles", async (
                string? searchTerm,
                ISender sender,
                CancellationToken cancellationToken,
                int pageSize = 10, 
                int page = 1) =>
            {
                var query = new GetVehiclesQuery(pageSize, page, searchTerm);

                Result<PagedList<VehicleDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<PagedList<VehicleDto>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}