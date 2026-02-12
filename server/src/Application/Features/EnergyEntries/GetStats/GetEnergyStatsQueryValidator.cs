using Application.Core;
using FluentValidation;

namespace Application.Features.EnergyEntries.GetStats;

internal sealed class GetEnergyStatsQueryValidator : AbstractValidator<GetEnergyStatsQuery>
{
    public GetEnergyStatsQueryValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithError(EnergyEntryErrors.VehicleIdRequired);
        
        RuleForEach(x => x.EnergyTypes)
            .IsInEnum()
            .WithError(EnergyEntryErrors.TypeInvalid);
    }
}