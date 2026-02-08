using Application.Abstractions;
using FluentValidation;

namespace Application.EnergyEntries.Update;

internal sealed class UpdateEnergyEntryCommandValidator : AbstractValidator<UpdateEnergyEntryCommand>
{
    public UpdateEnergyEntryCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Id)
            .NotEmpty();
        
        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(dateTimeProvider.UtcNow))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Mileage)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Mileage must be greater than 0.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .IsInEnum()
            .WithMessage("Invalid energy type.");

        RuleFor(x => x.EnergyUnit)
            .NotEmpty()
            .IsInEnum()
            .WithMessage("Invalid energy unit.");

        RuleFor(x => x.Volume)
            .GreaterThan(0)
            .WithMessage("Volume must be greater than 0.");

        RuleFor(x => x.Cost)
            .GreaterThan(0)
            .When(x => x.Cost.HasValue)
            .WithMessage("Cost must be greater than 0.");

        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .When(x => x.PricePerUnit.HasValue)
            .WithMessage("Price per unit must be greater than 0.");
    }
}