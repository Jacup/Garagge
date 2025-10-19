using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Vehicles.Update;

public sealed class UpdateVehicleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleUpdateValidationService validationService,
    ILogger<UpdateVehicleCommandHandler> logger)
    : ICommandHandler<UpdateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            logger.LogError("Attempt to update vehicle with empty userId");
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);
        }

        var vehicle = await dbContext.Vehicles
            .Include(v => v.VehicleEnergyTypes)
            .FirstOrDefaultAsync(v => v.UserId == userId && v.Id == request.VehicleId, cancellationToken);

        if (vehicle == null)
        {
            logger.LogError("Vehicle with Id = '{VehicleId}' not found for UserId = '{UserId}'", request.VehicleId, userId);
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound(request.VehicleId));
        }

        var energyTypesValidation = await validationService
            .ValidateEnergyTypesChangeAsync(
                vehicle,
                request.EnergyTypes,
                cancellationToken);

        if (energyTypesValidation.IsFailure)
            return Result.Failure<VehicleDto>(energyTypesValidation.Error);

        var updatePlan = energyTypesValidation.Value;

        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.EngineType = request.EngineType;
        vehicle.ManufacturedYear = request.ManufacturedYear;
        vehicle.Type = request.Type;
        vehicle.VIN = request.VIN;

        await ApplyEnergyTypesChanges(vehicle, updatePlan, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<VehicleDto>(VehicleErrors.ConcurrencyConflict(request.VehicleId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle {VehicleId}", request.VehicleId);
            return Result.Failure<VehicleDto>(VehicleErrors.UpdateFailed(request.VehicleId));
        }

        return vehicle.Adapt<VehicleDto>();
    }

    private async Task ApplyEnergyTypesChanges(Vehicle vehicle, VehicleEnergyTypesUpdatePlan plan, CancellationToken cancellationToken)
    {
        foreach (var typeToRemove in plan.TypesToRemove)
        {
            var vet = await dbContext.VehicleEnergyTypes
                .FirstOrDefaultAsync(v => v.VehicleId == vehicle.Id && v.EnergyType == typeToRemove, cancellationToken);

            if (vet != null)
            {
                dbContext.VehicleEnergyTypes.Remove(vet);
            }
        }

        foreach (var typeToAdd in plan.TypesToAdd)
        {
            await dbContext.VehicleEnergyTypes
                .AddAsync(new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = typeToAdd }, cancellationToken);
        }
    }
}