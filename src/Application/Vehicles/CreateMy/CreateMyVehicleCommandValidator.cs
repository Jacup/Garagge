using FluentValidation;

namespace Application.Vehicles.CreateMy;

internal sealed class CreateMyVehicleCommandValidator : AbstractValidator<CreateMyVehicleCommand>
{
    public CreateMyVehicleCommandValidator()
    {
        RuleFor(c => c.Brand)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("Brand is required.");
        
        RuleFor(c => c.Model)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("Model is required.");
        
        RuleFor(c => c.ManufacturedYear)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Manufactured year can't be in the future.");
    }
}