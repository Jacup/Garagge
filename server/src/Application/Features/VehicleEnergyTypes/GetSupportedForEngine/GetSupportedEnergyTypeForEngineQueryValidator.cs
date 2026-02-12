using Application.Core;
using Application.Features.Vehicles;
using FluentValidation;

namespace Application.Features.VehicleEnergyTypes.GetSupportedForEngine;

internal sealed class GetSupportedEnergyTypeForEngineQueryValidator : AbstractValidator<GetSupportedEnergyTypeForEngineQuery>
{
    public GetSupportedEnergyTypeForEngineQueryValidator()
    {
        RuleFor(q => q.EngineType)
            .IsInEnum()
            .WithError(VehicleErrors.EngineTypeInvalid);
    }
}