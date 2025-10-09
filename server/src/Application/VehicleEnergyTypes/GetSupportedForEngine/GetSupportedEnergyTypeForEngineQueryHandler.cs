using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Domain.Enums;

namespace Application.VehicleEnergyTypes.GetSupportedForEngine;

public sealed class GetSupportedEnergyTypeForEngineQueryHandler(IVehicleEngineCompatibilityService vehicleEngineCompatibilityService)
    : IQueryHandler<GetSupportedEnergyTypeForEngineQuery, ICollection<EnergyType>>
{
    public async Task<Result<ICollection<EnergyType>>> Handle(GetSupportedEnergyTypeForEngineQuery request, CancellationToken cancellationToken)
    {
        var supported = await vehicleEngineCompatibilityService.GetCompatibleEnergyTypesForEngine(request.EngineType);

        return Result.Success(supported);
    }
}