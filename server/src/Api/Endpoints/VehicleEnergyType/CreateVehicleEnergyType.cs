using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.VehicleEnergyTypes;
using Application.VehicleEnergyTypes.Create;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.VehicleEnergyType;

internal sealed class CreateVehicleEnergyType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/{vehicleId:guid}/energy-types",
                async (Guid vehicleId, CreateVehicleEnergyTypeRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateVehicleEnergyTypeCommand(vehicleId, request.Type);

                    Result<VehicleEnergyTypeDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(
                        vetDto => Results.Created($"/vehicles/{vetDto.VehicleId}/energy-types/{vetDto.Id}", vetDto),
                        CustomResults.Problem
                    );
                })
            .RequireAuthorization()
            .Produces<VehicleEnergyTypeDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.VehicleEnergyTypes);
    }
}

public sealed record CreateVehicleEnergyTypeRequest(EnergyType Type);