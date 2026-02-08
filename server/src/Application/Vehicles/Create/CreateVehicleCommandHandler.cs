using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;

namespace Application.Vehicles.Create;

internal sealed class CreateVehicleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext)
    : ICommandHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
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
        await dbContext.SaveChangesAsync(cancellationToken);

        return vehicle.Adapt<VehicleDto>();
    }
}