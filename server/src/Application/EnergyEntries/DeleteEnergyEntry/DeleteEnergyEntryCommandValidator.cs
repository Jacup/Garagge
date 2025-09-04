using Application.Abstractions;
using FluentValidation;

namespace Application.EnergyEntries.DeleteEnergyEntry;

internal sealed class DeleteEnergyEntryCommandValidator : AbstractValidator<DeleteEnergyEntryCommand>
{
    public DeleteEnergyEntryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Energy entry ID is required.");

        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required.");
    }
}