using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Features.EnergyEntries;
using Application.Features.EnergyEntries.GetByUser;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.EnergyEntries;

internal sealed class GetEnergyEntriesByUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/energy-entries", async (
                Guid userId,
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery]int page = 1,
                [FromQuery]int pageSize = 10,
                [FromQuery]EnergyType[]? energyTypes = null) =>
            {
                var query = new GetEnergyEntriesByUserQuery(userId, page, pageSize, energyTypes);

                Result<PagedList<EnergyEntryDto>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<PagedList<EnergyEntryDto>>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.EnergyEntries);
    }
}