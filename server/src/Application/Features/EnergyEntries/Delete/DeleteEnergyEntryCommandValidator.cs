using Application.Core;
using FluentValidation;

namespace Application.Features.EnergyEntries.Delete;

internal sealed class DeleteEnergyEntryCommandValidator : AbstractValidator<DeleteEnergyEntryCommand>
{
    public DeleteEnergyEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithError(EnergyEntryErrors.IdRequired);

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithError(EnergyEntryErrors.VehicleIdRequired);
    }
}
