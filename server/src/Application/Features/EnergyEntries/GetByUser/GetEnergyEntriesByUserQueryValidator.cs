using Application.Core;
using FluentValidation;

namespace Application.Features.EnergyEntries.GetByUser;

internal sealed class GetEnergyEntriesByUserQueryValidator : AbstractValidator<GetEnergyEntriesByUserQuery>
{
    public GetEnergyEntriesByUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithError(EnergyEntryErrors.UserIdRequired);

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
