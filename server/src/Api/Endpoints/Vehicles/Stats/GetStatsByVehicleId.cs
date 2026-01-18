using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles.Stats;
using Application.Vehicles.Stats.GetByVehicleId;
using MediatR;

namespace Api.Endpoints.Vehicles.Stats;

internal sealed class GetStatsByVehicleId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/{id:guid}/stats", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetStatsByVehicleIdQuery(id);

                Result<VehicleStatsDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<VehicleStatsDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}