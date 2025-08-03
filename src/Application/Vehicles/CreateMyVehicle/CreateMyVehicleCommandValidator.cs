using Application.Abstractions;
using FluentValidation;

namespace Application.Vehicles.CreateMyVehicle;

internal sealed class CreateMyVehicleCommandValidator : AbstractValidator<CreateMyVehicleCommand>
{
    public CreateMyVehicleCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(c => c.Brand)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("Brand is required and must not exceed 64 characters.");
        
        RuleFor(c => c.Model)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("Model is required and must not exceed 64 characters.");

        const int firstCarManufacturedYear = 1886;
        
        RuleFor(c => c.ManufacturedYear)
            .GreaterThanOrEqualTo(firstCarManufacturedYear)
            .LessThanOrEqualTo(dateTimeProvider.UtcNow.Year)
            .WithMessage($"Manufactured year must be between {firstCarManufacturedYear} and the current year.");
    }
}