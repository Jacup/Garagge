using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Mapster;

namespace Application.Vehicles.Create;

internal sealed class CreateVehicleCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

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

        await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);

        if (request.EnergyTypes is not null && request.EnergyTypes.Any())
        {
            var vehicleEnergyTypes = CreateVehicleEnergyTypes(request.EnergyTypes, vehicle.Id).ToList();
            await dbContext.VehicleEnergyTypes.AddRangeAsync(vehicleEnergyTypes, cancellationToken);

            vehicle.VehicleEnergyTypes = vehicleEnergyTypes;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<VehicleDto>(VehicleErrors.CreateFailed);
        }

        return Result.Success(vehicle.Adapt<VehicleDto>());
    }

    private static IEnumerable<VehicleEnergyType> CreateVehicleEnergyTypes(IEnumerable<EnergyType> energyTypes, Guid vehicleId)
    {
        return energyTypes
            .Distinct()
            .Select(energyType => new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicleId, EnergyType = energyType });
    }
}