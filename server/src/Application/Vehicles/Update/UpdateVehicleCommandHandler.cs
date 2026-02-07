using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.Update;

public sealed class UpdateVehicleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleUpdateValidationService validationService)
    : ICommandHandler<UpdateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var vehicle = await dbContext.Vehicles
            .Include(v => v.VehicleEnergyTypes)
            .FirstOrDefaultAsync(v => v.UserId == userId &&
                                      v.Id == request.VehicleId, cancellationToken);

        if (vehicle == null)
            return Result.Failure<VehicleDto>(VehicleErrors.NotFound);

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
            return Result.Failure<VehicleDto>(VehicleErrors.ConcurrencyConflict);
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