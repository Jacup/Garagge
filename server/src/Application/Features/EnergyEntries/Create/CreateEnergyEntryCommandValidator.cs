using Application.Abstractions;
using Application.Core;
using FluentValidation;

namespace Application.Features.EnergyEntries.Create;

internal sealed class CreateEnergyEntryCommandValidator : AbstractValidator<CreateEnergyEntryCommand>
{
    public CreateEnergyEntryCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithError(EnergyEntryErrors.VehicleIdRequired);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithError(EnergyEntryErrors.DateRequired)
            .LessThanOrEqualTo(DateOnly.FromDateTime(dateTimeProvider.UtcNow))
            .WithError(EnergyEntryErrors.DateInFuture);

        RuleFor(x => x.Mileage)
            .NotEmpty()
            .WithError(EnergyEntryErrors.MileageRequired)
            .GreaterThan(0)
            .WithError(EnergyEntryErrors.MileageInvalid);

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithError(EnergyEntryErrors.TypeRequired)
            .IsInEnum()
            .WithError(EnergyEntryErrors.TypeInvalid);

        RuleFor(x => x.EnergyUnit)
            .NotEmpty()
            .WithError(EnergyEntryErrors.EnergyUnitRequired)
            .IsInEnum()
            .WithError(EnergyEntryErrors.EnergyUnitInvalid);

        RuleFor(x => x.Volume)
            .GreaterThan(0)
            .WithError(EnergyEntryErrors.VolumeInvalid);

        RuleFor(x => x.Cost)
            .GreaterThan(0)
            .When(x => x.Cost.HasValue)
            .WithError(EnergyEntryErrors.CostInvalid);

        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .When(x => x.PricePerUnit.HasValue)
            .WithError(EnergyEntryErrors.PricePerUnitInvalid);
    }
}
