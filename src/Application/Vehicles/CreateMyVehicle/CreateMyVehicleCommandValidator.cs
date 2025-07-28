using FluentValidation;

namespace Application.Vehicles.CreateMyVehicle;

internal sealed class CreateMyVehicleCommandValidator : AbstractValidator<CreateMyVehicleCommand>
{
    public CreateMyVehicleCommandValidator()
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
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Manufactured year can't be in the future.");
    }
}