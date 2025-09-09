using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries;
using Application.EnergyEntries.GetByVehicle;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.EnergyEntries;

internal sealed class GetEnergyEntriesByVehicle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/{vehicleId:guid}/energy-entries", async (
                Guid vehicleId,
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery]int page = 1,
                [FromQuery]int pageSize = 10,
                [FromQuery]EnergyType[]? energyTypes = null) =>
            {
                var query = new GetEnergyEntriesByVehicleQuery(vehicleId, page, pageSize, energyTypes);

                Result<PagedList<EnergyEntryDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .Produces<PagedList<EnergyEntryDto>>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.EnergyEntries);
    }
}
