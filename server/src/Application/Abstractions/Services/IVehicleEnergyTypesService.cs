using Application.Core;
using Application.Services;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleEnergyTypesService
{
    Task<Result<VehicleEnergyTypesUpdateResult>> PrepareUpdateAsync(
        Vehicle vehicle,
        IEnumerable<EnergyType> requestedEnergyTypes,
        CancellationToken cancellationToken);
}