using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries.CreateChargingEntry;
using Application.EnergyEntries.Dtos;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

public class CreateChargingEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/{vehicleId:guid}/charging-entries",
                async (CreateChargingEntryRequest request, Guid vehicleId, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new CreateChargingEntryCommand(
                        vehicleId,
                        request.Date,
                        request.Mileage,
                        request.Cost,
                        request.PricePerUnit,
                        request.EnergyAmount,
                        request.Unit,
                        request.ChargingDurationMinutes
                    );

                    Result<ChargingEntryDto> result = await sender.Send(command, cancellationToken);

                    return result.Match(
                        chargingEntry => Results.Created($"/vehicles/{chargingEntry.VehicleId}/charging-entries/{chargingEntry.Id}", chargingEntry),
                        CustomResults.Problem
                    );
                })
            .Produces<ChargingEntryDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Vehicles);
    }
}

public record CreateChargingEntryRequest(
    DateOnly Date,
    int Mileage,
    decimal Cost,
    decimal PricePerUnit,
    decimal EnergyAmount,
    EnergyUnit Unit,
    int? ChargingDurationMinutes);

