using Application.Abstractions;
using FluentValidation;

namespace Application.EnergyEntries.CreateFuelEntry;

internal sealed class CreateFuelEntryCommandValidator : AbstractValidator<CreateFuelEntryCommand>
{
    public CreateFuelEntryCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(dateTimeProvider.UtcNow))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Mileage)
            .GreaterThan(0)
            .WithMessage("Mileage must be greater than 0.");

        RuleFor(x => x.Cost)
            .GreaterThan(0)
            .WithMessage("Cost must be greater than 0.");

        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .WithMessage("Price per unit must be greater than 0.");

        RuleFor(x => x.Volume)
            .GreaterThan(0)
            .WithMessage("Volume must be greater than 0.");

        RuleFor(x => x.Unit)
            .IsInEnum()
            .WithMessage("Unit must be valid enum value");
    }
}