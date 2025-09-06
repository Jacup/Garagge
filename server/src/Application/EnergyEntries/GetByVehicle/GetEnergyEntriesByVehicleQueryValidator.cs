using FluentValidation;

namespace Application.EnergyEntries.GetByVehicle;

internal sealed class GetEnergyEntriesByVehicleQueryValidator : AbstractValidator<GetEnergyEntriesByVehicleQuery>
{
    public GetEnergyEntriesByVehicleQueryValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage("Vehicle ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.EnergyType)
            .IsInEnum()
            .When(x => x.EnergyType.HasValue)
            .WithMessage("Invalid energy type.");
    }
}