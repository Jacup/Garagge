using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Application.Vehicles;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.VehicleEnergyTypes.Create;

internal sealed class CreateVehicleEnergyTypeCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleEngineCompatibilityService vehicleEngineCompatibilityService)
    : ICommandHandler<CreateVehicleEnergyTypeCommand, VehicleEnergyTypeDto>
{
    public async Task<Result<VehicleEnergyTypeDto>> Handle(CreateVehicleEnergyTypeCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.Unauthorized);

        var validationResult = await ValidateRequestAsync(userId, request, cancellationToken);
        
        if (validationResult.IsFailure)
            return validationResult;

        var vehicleEnergyType = new VehicleEnergyType { VehicleId = request.VehicleId, EnergyType = request.EnergyType };

        try
        {
            await dbContext.VehicleEnergyTypes.AddAsync(vehicleEnergyType, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.CreateFailed);
        }

        return Result.Success(vehicleEnergyType.Adapt<VehicleEnergyTypeDto>());
    }

    private async Task<Result<VehicleEnergyTypeDto>> ValidateRequestAsync(Guid userId, CreateVehicleEnergyTypeCommand request,
        CancellationToken cancellationToken)
    {
        var validationQuery = await dbContext.Vehicles
            .Where(v => v.Id == request.VehicleId)
            .Select(v => new
            {
                Vehicle = new { v.Id, v.UserId, v.EngineType },
                ExistingEnergyType = v.VehicleEnergyTypes
                    .Any(vet => vet.EnergyType == request.EnergyType)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (validationQuery == null)
            return Result.Failure<VehicleEnergyTypeDto>(VehicleErrors.NotFound(request.VehicleId));

        if (validationQuery.Vehicle.UserId != userId)
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.Unauthorized);

        if (validationQuery.ExistingEnergyType)
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.AlreadyExists(request.VehicleId, request.EnergyType));

        if (!vehicleEngineCompatibilityService.IsEnergyTypeCompatibleWithEngine(request.EnergyType, validationQuery.Vehicle.EngineType))
            return Result.Failure<VehicleEnergyTypeDto>(VehicleEnergyTypeErrors.IncompatibleWithEngine(request.EnergyType, validationQuery.Vehicle.EngineType));

        return Result.Success<VehicleEnergyTypeDto>(null!);
    }
}