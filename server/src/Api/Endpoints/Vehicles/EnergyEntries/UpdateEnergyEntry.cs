using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries;
using Application.EnergyEntries.Update;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

internal sealed class UpdateEnergyEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "vehicles/{vehicleId:guid}/energy-entries/{id:guid}",
                async (UpdateEnergyEntryRequest request, Guid vehicleId, Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateEnergyEntryCommand(
                        vehicleId,
                        id,
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
            .Produces<EnergyEntryDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.EnergyEntries);
    }
}

public record UpdateEnergyEntryRequest(
        DateOnly Date,
        int Mileage,
        EnergyType Type,
        EnergyUnit EnergyUnit,
        decimal Volume,
        decimal? Cost,
        decimal? PricePerUnit);