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
        
        RuleFor(c => c.ManufacturedYear)
            .GreaterThanOrEqualTo(1886)
            .LessThanOrEqualTo(dateTimeProvider.UtcNow.Year)
            .WithMessage("Manufactured year must be between 1886 and the current year.");
    }
}