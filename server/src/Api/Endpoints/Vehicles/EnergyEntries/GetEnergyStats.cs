using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Features.EnergyEntries;
using Application.Features.EnergyEntries.GetStats;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.EnergyEntries;

internal sealed class GetEnergyStats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/{vehicleId:guid}/energy-entries/stats", async (
                Guid vehicleId,
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery] EnergyType[]? energyTypes) =>
            {
                var query = new GetEnergyStatsQuery(vehicleId, energyTypes ?? []);

                Result<EnergyStatsDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<EnergyStatsDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.EnergyEntries);
    }
}