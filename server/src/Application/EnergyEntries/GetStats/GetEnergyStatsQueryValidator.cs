using FluentValidation;

namespace Application.EnergyEntries.GetStats;

internal sealed class GetEnergyStatsQueryValidator : AbstractValidator<GetEnergyStatsQuery>
{
    public GetEnergyStatsQueryValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required.");
        
        RuleForEach(x => x.EnergyTypes)
            .IsInEnum()
            .WithMessage("Invalid energy type.");
    }
}