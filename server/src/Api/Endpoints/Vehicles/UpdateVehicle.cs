using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.Update;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles;

internal sealed class UpdateVehicle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "vehicles/{id:guid}",
                async (Guid id, VehicleUpdateRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateVehicleCommand
                    (
                        id,
                        request.Brand,
                        request.Model,
                        request.EngineType,
                        request.EnergyTypes ?? [],
                        request.ManufacturedYear,
                        request.Type,
                        request.VIN
                    );

                    Result<VehicleDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<VehicleDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}

internal sealed record VehicleUpdateRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear,
    VehicleType? Type,
    string? VIN,
    IEnumerable<EnergyType>? EnergyTypes);

