using Api.Extensions;
using Api.Infrastructure;
using Application.VehicleEnergyTypes.GetSupportedForEngine;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.VehicleEnergyType;

public class GetSupportedVehicleEnergyType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/energy-types/supported",
                async (EngineType engineType, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetSupportedEnergyTypeForEngineQuery(engineType);
                    var result = await sender.Send(query, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<ICollection<EnergyType>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.VehicleEnergyTypes);
    }
}