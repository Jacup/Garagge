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
        
        RuleFor(x => x.Period)
            .IsInEnum()
            .WithError(EnergyEntryErrors.PeriodInvalid);
    }
}