using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.Create;

internal sealed class CreateEnergyEntryCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleEngineCompatibilityService engineCompatibilityService)
    : ICommandHandler<CreateEnergyEntryCommand, EnergyEntryDto>
{
    public async Task<Result<EnergyEntryDto>> Handle(CreateEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId && v.UserId == userContext.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle is null)
            return Result.Failure<EnergyEntryDto>(VehicleErrors.NotFound);

        var isCompatible = await engineCompatibilityService.IsEnergyTypeCompatibleAsync(vehicle.Id, request.Type, cancellationToken);

        if (!isCompatible)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.TypeIncompatible);

        var lastEntry = await dbContext.EnergyEntries
            .Where(ee => ee.VehicleId == request.VehicleId)
            .OrderByDescending(ee => ee.Mileage)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastEntry != null && request.Mileage < lastEntry.Mileage)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.MileageIncorrect);

        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            Vehicle = null!,
            Date = request.Date,
            Mileage = request.Mileage,
            Type = request.Type,
            EnergyUnit = request.EnergyUnit,
            Volume = request.Volume,
            Cost = request.Cost,
            PricePerUnit = request.PricePerUnit,
        };

        await dbContext.EnergyEntries.AddAsync(energyEntry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return energyEntry.Adapt<EnergyEntryDto>();
    }
}