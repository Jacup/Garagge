using Domain.Enums;
using FluentValidation;

namespace Application.VehicleEnergyTypes.GetSupportedForEngine;

internal sealed class GetSupportedEnergyTypeForEngineQueryValidator : AbstractValidator<GetSupportedEnergyTypeForEngineQuery>
{
    public GetSupportedEnergyTypeForEngineQueryValidator()    
    {
        RuleFor(q => q.EngineType)
            .IsInEnum()
            .WithMessage($"Engine Type must be a valid enum value. Valid values are: {string.Join(", ", Enum.GetNames<EngineType>())}.");
    }
}