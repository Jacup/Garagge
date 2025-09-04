using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.EnergyEntries.CreateFuelEntry;
using Application.EnergyEntries.Dtos;
using Application.Vehicles;
using Domain.Enums;
using MediatR;

namespace Api.Endpoints.Vehicles.EnergyEntries;

public class CreateFuelEntry : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/{vehicleId:guid}/fuel-entries",
                async (CreateFuelEntryRequest request, Guid vehicleId, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new CreateFuelEntryCommand(
                    vehicleId,
                    request.Date,
                    request.Mileage,
                    request.Cost,
                    request.Volume,
                    request.Unit,
                    request.PricePerUnit
                );
                
                Result<FuelEntryDto> result = await sender.Send(command, cancellationToken);

                return result.Match(
                    fuelEntry => Results.Created($"/vehicles/{fuelEntry.VehicleId}/fuel-entries/{fuelEntry.Id}", fuelEntry),
                    CustomResults.Problem
                );
            })
            .Produces<VehicleDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError)
            .HasPermission(Permissions.UsersAccess)
            .WithTags(Tags.Vehicles);
    }
}

public record CreateFuelEntryRequest(
    DateOnly Date,
    int Mileage,
    decimal Cost,
    decimal Volume,
    VolumeUnit Unit,
    decimal PricePerUnit);

