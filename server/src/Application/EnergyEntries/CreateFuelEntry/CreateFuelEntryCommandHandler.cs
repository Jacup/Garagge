using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.EnergyEntries.Dtos;
using Domain.Entities.EnergyEntries;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.EnergyEntries.CreateFuelEntry;

public class CreateFuelEntryCommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IVehicleEnergyValidator vehicleEnergyValidator)
    : ICommandHandler<CreateFuelEntryCommand, FuelEntryDto>
{
    public async Task<Result<FuelEntryDto>> Handle(CreateFuelEntryCommand request, CancellationToken cancellationToken)
    {
        var vehicleData = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == request.VehicleId)
            .Select(v => new { v.UserId , v.PowerType})
            .FirstOrDefaultAsync(cancellationToken);
        
        if (vehicleData is null)
            return Result.Failure<FuelEntryDto>(EnergyEntryErrors.NotFound(request.VehicleId));
        
        if (userContext.UserId != vehicleData.UserId)
            return Result.Failure<FuelEntryDto>(EnergyEntryErrors.Unauthorized);

        if (!vehicleEnergyValidator.CanBeFueled(vehicleData.PowerType))
            return Result.Failure<FuelEntryDto>(EnergyEntryErrors.IncompatiblePowerType(request.VehicleId ,vehicleData.PowerType));
        
        var fuelEntry = new FuelEntry
        {
            Id = Guid.NewGuid(),
            Date = request.Date,
            Mileage = request.Mileage,
            Cost = request.Cost,
            Volume = request.Volume,
            Unit = request.Unit,
            PricePerUnit = request.PricePerUnit,
            VehicleId = request.VehicleId
        };

        try
        {
            await dbContext.FuelEntries.AddAsync(fuelEntry, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<FuelEntryDto>(EnergyEntryErrors.CreateFailed);
        }
        
        return Result.Success(fuelEntry.Adapt<FuelEntryDto>());
    }
}