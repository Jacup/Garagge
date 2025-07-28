using FluentValidation;

namespace Application.Vehicles.GetMyVehicles;

internal sealed class GetMyVehiclesQueryValidator : AbstractValidator<GetMyVehicles>
{
    public GetMyVehiclesQueryValidator()
    {
        RuleFor(q => q.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}

