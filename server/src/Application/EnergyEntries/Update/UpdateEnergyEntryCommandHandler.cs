using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.Update;

internal sealed class UpdateEnergyEntryCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleEngineCompatibilityService engineCompatibilityService,
    IEnergyEntryMileageValidator mileageValidator)
    : ICommandHandler<UpdateEnergyEntryCommand, EnergyEntryDto>
{
    public async Task<Result<EnergyEntryDto>> Handle(UpdateEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var energyEntry = await dbContext.EnergyEntries
            .Include(ee => ee.Vehicle)
            .FirstOrDefaultAsync(ee =>
                    ee.Id == request.Id &&
                    ee.VehicleId == request.VehicleId &&
                    ee.Vehicle.UserId == userContext.UserId,
                cancellationToken);

        if (energyEntry is null)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.NotFound);

        var isCompatible = await engineCompatibilityService.IsEnergyTypeCompatibleAsync(energyEntry.VehicleId, request.Type, cancellationToken);

        if (!isCompatible)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.TypeIncompatible);

        var entries = await dbContext.EnergyEntries
            .Where(ee => ee.VehicleId == energyEntry.VehicleId)
            .ToListAsync(cancellationToken);

        if (!mileageValidator.IsValid(entries, energyEntry, request.Date, request.Mileage))
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.MileageIncorrect);

        energyEntry.Date = request.Date;
        energyEntry.Mileage = request.Mileage;
        energyEntry.Type = request.Type;
        energyEntry.EnergyUnit = request.EnergyUnit;
        energyEntry.Volume = request.Volume;
        energyEntry.Cost = request.Cost;
        energyEntry.PricePerUnit = request.PricePerUnit;

        await dbContext.SaveChangesAsync(cancellationToken);

        return energyEntry.Adapt<EnergyEntryDto>();
    }
}