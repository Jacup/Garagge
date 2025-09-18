using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Application.Vehicles.Create;

internal sealed class CreateVehicleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IVehicleEngineCompatibilityService vehicleEngineCompatibilityService,
    ILogger<CreateVehicleCommandHandler> logger)
    : ICommandHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

        var energyTypes = request.EnergyTypes?.Distinct().ToList();

        if (energyTypes?.Count == 0)
        {
            var validationResult = vehicleEngineCompatibilityService.ValidateEngineCompatibility(request.EngineType, energyTypes);
            
            if (validationResult.IsFailure)
                return Result.Failure<VehicleDto>(validationResult.Error);
        }
        
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = request.Brand,
            Model = request.Model,
            EngineType = request.EngineType,
            ManufacturedYear = request.ManufacturedYear,
            Type = request.Type,
            VIN = request.VIN,
            UserId = userId,
        };
        
        var vets = energyTypes?
            .Select(et => new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = et })
            .ToList();

        vehicle.VehicleEnergyTypes = vets ?? [];

        await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            logger.LogError("Error occurred while creating vehicle {Vehicle.Brand} {Vehicle.Model}", request.Brand, request.Model);
            return Result.Failure<VehicleDto>(VehicleErrors.CreateFailed);
        }

        return Result.Success(vehicle.Adapt<VehicleDto>());
    }
}