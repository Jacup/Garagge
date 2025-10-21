using Application.Core;
using Application.Vehicles.Update;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace Application.Abstractions.Services;

public interface IVehicleUpdateValidationService
{
    Task<Result<VehicleEnergyTypesUpdatePlan>> ValidateEnergyTypesChangeAsync(
        Vehicle vehicle,
        IEnumerable<EnergyType> requestedEnergyTypes,
        CancellationToken cancellationToken);
}