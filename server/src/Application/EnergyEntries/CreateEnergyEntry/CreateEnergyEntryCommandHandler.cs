using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.CreateEnergyEntry;

public class CreateEnergyEntryCommandHandler(
    IApplicationDbContext dbContext, 
    IUserContext userContext,
    IVehicleEnergyCompatibilityService energyCompatibilityService)
    : ICommandHandler<CreateEnergyEntryCommand, EnergyEntryDto>
{
    public async Task<Result<EnergyEntryDto>> Handle(CreateEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (vehicle is null)
            return Result.Failure<EnergyEntryDto>(VehicleErrors.NotFound(request.VehicleId));
        
        if (userContext.UserId != vehicle.UserId)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.Unauthorized);

        var isCompatible = await energyCompatibilityService.IsEnergyTypeCompatibleAsync(request.VehicleId, request.Type, cancellationToken);

        if (!isCompatible)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.IncompatibleEnergyType(request.VehicleId, request.Type));

        var lastEntry = await dbContext.EnergyEntries
                    .Where(ee => ee.VehicleId == request.VehicleId)
                    .OrderByDescending(ee => ee.Mileage)
                    .FirstOrDefaultAsync(cancellationToken);

        if (lastEntry != null && request.Mileage < lastEntry.Mileage)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.IncorrectMileage);

        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            Date = request.Date,
            Mileage = request.Mileage,
            Type = request.Type,
            EnergyUnit = request.EnergyUnit,
            Volume = request.Volume,
            Cost = request.Cost,
            PricePerUnit = request.PricePerUnit,
        };

        try
        {
            await dbContext.EnergyEntries.AddAsync(energyEntry, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.CreateFailed);
        }
        
        return Result.Success(energyEntry.Adapt<EnergyEntryDto>());
    }
}