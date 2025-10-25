using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.Create;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles;

internal sealed class CreateVehicle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles",
                async (VehicleCreateRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateVehicleCommand
                    (
                        request.Brand,
                        request.Model,
                        request.EngineType,
                        request.EnergyTypes ?? [],
                        request.ManufacturedYear,
                        request.Type,
                        request.VIN
                    );

                    Result<VehicleDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(
                        vehicle => Results.Created($"/vehicles/{vehicle.Id}", vehicle),
                        CustomResults.Problem
                    );
                })
            .RequireAuthorization()
            .Produces<VehicleDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}

internal sealed record VehicleCreateRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear,
    VehicleType? Type,
    string? VIN,
    IEnumerable<EnergyType>? EnergyTypes);

