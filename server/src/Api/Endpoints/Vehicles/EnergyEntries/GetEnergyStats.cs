using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries;
using Application.EnergyEntries.GetStats;
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
            .Produces<EnergyStatsDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.EnergyEntries);
    }
}