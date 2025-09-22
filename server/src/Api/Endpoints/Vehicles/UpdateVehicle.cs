using Api.Endpoints.Users;
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
                async (Guid id, UpdateVehicleRequest request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateVehicleCommand
                    (
                        id,
                        request.Brand,
                        request.Model,
                        request.EngineType,
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

record UpdateVehicleRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null);