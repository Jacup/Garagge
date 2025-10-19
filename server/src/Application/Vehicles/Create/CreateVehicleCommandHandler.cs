using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Application.Vehicles.Create;

public sealed class CreateVehicleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    ILogger<CreateVehicleCommandHandler> logger)
    : ICommandHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Attempt to create vehicle with empty userId");
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);
        }

        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = vehicleId,
            Brand = request.Brand,
            Model = request.Model,
            EngineType = request.EngineType,
            ManufacturedYear = request.ManufacturedYear,
            Type = request.Type,
            VIN = request.VIN,
            UserId = userId,
        };

        foreach (var energyType in request.EnergyTypes)
        {
            vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicleId, EnergyType = energyType });
        }

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

        logger.LogInformation(
            "Vehicle created successfully. VehicleId: {VehicleId}, UserId: {UserId}, Brand: {Brand}, Model: {Model}",
            vehicleId,
            userId,
            request.Brand,
            request.Model);

        return vehicle.Adapt<VehicleDto>();
    }
}