using Application.Core;
using FluentValidation;

namespace Application.Features.EnergyEntries.GetByVehicle;

internal sealed class GetEnergyEntriesByVehicleQueryValidator : AbstractValidator<GetEnergyEntriesByVehicleQuery>
{
    public GetEnergyEntriesByVehicleQueryValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithError(EnergyEntryErrors.VehicleIdRequired);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithError(EnergyEntryErrors.PageInvalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(EnergyEntryErrors.PageSizeInvalid);

        When(x => x.EnergyTypes != null, () =>
        {
            RuleForEach(x => x.EnergyTypes)
                .IsInEnum()
                .WithError(EnergyEntryErrors.TypeInvalid);
        });
    }
}
