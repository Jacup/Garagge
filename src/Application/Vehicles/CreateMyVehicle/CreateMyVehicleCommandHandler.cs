using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;

namespace Application.Vehicles.CreateMyVehicle;

internal sealed class CreateMyVehicleCommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateMyVehicleCommand, VehicleDto>
{
    public async Task<Result<VehicleDto>> Handle(CreateMyVehicleCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var now = dateTimeProvider.UtcNow;
        
        var vehicle = new Vehicle()
        {
            Id = Guid.NewGuid(),
            Brand = request.Brand,
            Model = request.Model,
            ManufacturedYear = request.ManufacturedYear,
            UserId = userId,
            CreatedDate = now,
            UpdatedDate = now
        };

        await dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return vehicle.Adapt<VehicleDto>();
    }
}