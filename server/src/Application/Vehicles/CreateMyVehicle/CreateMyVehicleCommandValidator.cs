using Application.Abstractions;
using Domain.Enums;
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
        
        RuleFor(c => c.PowerType)
            .IsInEnum()
            .WithMessage($"Power type must be a valid enum value. Valid values are: {string.Join(", ", Enum.GetNames<PowerType>())}.");

        const int firstCarManufacturedYear = 1886;
        
        RuleFor(c => c.ManufacturedYear)
            .GreaterThanOrEqualTo(firstCarManufacturedYear)
            .LessThanOrEqualTo(dateTimeProvider.UtcNow.Year)
            .When(c => c.ManufacturedYear.HasValue)
            .WithMessage($"Manufactured year must be between {firstCarManufacturedYear} and the current year.");
        
        RuleFor(c => c.Type)
            .IsInEnum()
            .When(c => c.Type.HasValue)
            .WithMessage($"Vehicle type must be a valid enum value. Valid values are: {string.Join(", ", Enum.GetNames<VehicleType>())}.");

        RuleFor(c => c.VIN)
            .Length(17)
            .When(c => c.VIN is not null)
            .WithMessage("VIN must be exactly 17 characters long.");
    }
}