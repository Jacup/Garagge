using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries;
using Application.EnergyEntries.GetByUser;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

public class GetEnergyEntriesByUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/energy-entries", async (
                Guid userId, 
                ISender sender, 
                CancellationToken cancellationToken,
                int page = 1,
                int pageSize = 10,
                EnergyType? energyType = null) =>
            {
                var query = new GetEnergyEntriesByUserQuery(userId, page, pageSize, energyType);

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