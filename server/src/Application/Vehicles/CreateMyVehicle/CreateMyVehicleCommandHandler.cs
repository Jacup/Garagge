using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;

namespace Application.Vehicles.CreateMyVehicle;

internal sealed class CreateMyVehicleCommandHandler(IApplicationDbContext dbContext, IUserContext userContext)
    : ICommandHandler<CreateMyVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(CreateMyVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = request.Brand,
            Model = request.Model,
            PowerType = request.PowerType,
            ManufacturedYear = request.ManufacturedYear,
            Type = request.Type,
            VIN = request.VIN,
            UserId = userId,
        };
        
        try
        {
            await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<VehicleDto>(VehicleErrors.CreateFailed);
        }

        return Result.Success(vehicle.Adapt<VehicleDto>());
    }
}