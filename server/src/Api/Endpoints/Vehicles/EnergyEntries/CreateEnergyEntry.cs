using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries;
using Application.EnergyEntries.Create;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

public class CreateEnergyEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/{vehicleId:guid}/energy-entries",
                async (CreateEnergyEntryRequest request, Guid vehicleId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateEnergyEntryCommand(
                        vehicleId,
                        request.Date,
                        request.Mileage,
                        request.Type,
                        request.EnergyUnit,
                        request.Volume,
                        request.Cost,
                        request.PricePerUnit
                    );

                    Result<EnergyEntryDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(
                        chargingEntry => Results.Created($"/vehicles/{chargingEntry.VehicleId}/energy-entries/{chargingEntry.Id}", chargingEntry),
                        CustomResults.Problem
                    );
                })
            .Produces<EnergyEntryDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.EnergyEntries);
    }
}

public record CreateEnergyEntryRequest(
        DateOnly Date,
        int Mileage,
        EnergyType Type,
        EnergyUnit EnergyUnit,
        decimal Volume,
        decimal? Cost,
        decimal? PricePerUnit);

