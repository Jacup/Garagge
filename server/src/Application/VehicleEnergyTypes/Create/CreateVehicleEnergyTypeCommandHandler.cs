using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.VehicleEnergyTypes.Create;

internal sealed class CreateVehicleEnergyTypeCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleEngineCompatibilityService vehicleEngineCompatibilityService,
    ILogger<CreateVehicleEnergyTypeCommandHandler> logger)
    : ICommandHandler<CreateVehicleEnergyTypeCommand, VehicleEnergyTypeDto>
{
    public async Task<Result<VehicleEnergyTypeDto>> Handle(CreateVehicleEnergyTypeCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.Unauthorized);

        var vehicle = await dbContext.Vehicles
            .Where(v =>
                v.Id == request.VehicleId &&
                v.UserId == userId)
            .Include(v => v.VehicleEnergyTypes)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicle == null)
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.NotFound(request.VehicleId));

        var validationResult = vehicleEngineCompatibilityService.ValidateEnergyTypeAssignment(vehicle, request.EnergyType);
        if (validationResult.IsFailure)
            return Result.Failure<VehicleEnergyTypeDto>(validationResult.Error);

        var vehicleEnergyType = new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = request.VehicleId, EnergyType = request.EnergyType };

        await dbContext.VehicleEnergyTypes.AddAsync(vehicleEnergyType, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            logger.LogError("Error creating vehicle energy type for vehicle {VehicleId} and energy type {EnergyType}", request.VehicleId, request.EnergyType);
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.CreateFailed);
        }

        return Result.Success(vehicleEnergyType.Adapt<VehicleEnergyTypeDto>());
    }
}