using Application.Abstractions;
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
    IVehicleEnergyCompatibilityService energyCompatibilityService,
    IEnergyEntryMileageValidator mileageValidator)
    : ICommandHandler<UpdateEnergyEntryCommand, EnergyEntryDto>
{
    public async Task<Result<EnergyEntryDto>> Handle(UpdateEnergyEntryCommand request, CancellationToken cancellationToken)
    {
        var actualEnergyEntry = await dbContext.EnergyEntries
            .Include(ee => ee.Vehicle)
            .FirstOrDefaultAsync(ee => ee.Id == request.Id, cancellationToken);

        if (actualEnergyEntry is null)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.NotFound(request.Id));
        
        if (actualEnergyEntry.VehicleId != request.VehicleId)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.Invalid);

        if (actualEnergyEntry.Vehicle!.UserId != userContext.UserId)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.Unauthorized);

        var isCompatible = await energyCompatibilityService.IsEnergyTypeCompatibleAsync(actualEnergyEntry.VehicleId, request.Type, cancellationToken);

        if (!isCompatible)
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.IncompatibleEnergyType(actualEnergyEntry.VehicleId, request.Type));
        
        var entries = await dbContext.EnergyEntries
            .Where(ee => ee.VehicleId == actualEnergyEntry.VehicleId)
            .ToListAsync(cancellationToken);
        
        if (!mileageValidator.IsValid(entries, actualEnergyEntry, request.Date, request.Mileage))
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.IncorrectMileage);

        actualEnergyEntry.Date = request.Date;
        actualEnergyEntry.Mileage = request.Mileage;
        actualEnergyEntry.Type = request.Type;
        actualEnergyEntry.EnergyUnit = request.EnergyUnit;
        actualEnergyEntry.Volume = request.Volume;
        actualEnergyEntry.Cost = request.Cost;
        actualEnergyEntry.PricePerUnit = request.PricePerUnit;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<EnergyEntryDto>(EnergyEntryErrors.CreateFailed);
        }

        return Result.Success(actualEnergyEntry.Adapt<EnergyEntryDto>());
    }
}