using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.EnergyEntries.Dtos;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.CreateChargingEntry;

public class CreateChargingEntryCommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IVehicleEnergyValidator vehicleEnergyValidator)
    : ICommandHandler<CreateChargingEntryCommand, ChargingEntryDto>
{
    public async Task<Result<ChargingEntryDto>> Handle(CreateChargingEntryCommand request, CancellationToken cancellationToken)
    {
        var vehicleData = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .Select(v => new { v.UserId , v.PowerType})
            .FirstOrDefaultAsync(cancellationToken);
        
        if (vehicleData is null)
            return Result.Failure<ChargingEntryDto>(VehicleErrors.NotFound(request.VehicleId));
        
        if (userContext.UserId != vehicleData.UserId)
            return Result.Failure<ChargingEntryDto>(FuelEntryErrors.Unauthorized);

        if (!vehicleEnergyValidator.CanBeCharged(vehicleData.PowerType))
            return Result.Failure<ChargingEntryDto>(FuelEntryErrors.IncompatiblePowerType(request.VehicleId ,vehicleData.PowerType));
        
        var energyEntry = new ChargingEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            Date = request.Date,
            Mileage = request.Mileage,
            Cost = request.Cost,
            EnergyAmount = request.EnergyAmount,
            Unit = request.Unit,
            PricePerUnit = request.PricePerUnit,
            ChargingDurationMinutes = request.ChargingDurationMinutes,
        };

        try
        {
            await dbContext.ChargingEntries.AddAsync(energyEntry, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<ChargingEntryDto>(FuelEntryErrors.CreateFailed);
        }
        
        return Result.Success(energyEntry.Adapt<ChargingEntryDto>());
    }
}