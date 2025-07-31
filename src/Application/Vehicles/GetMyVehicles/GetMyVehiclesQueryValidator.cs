using FluentValidation;

namespace Application.Vehicles.GetMyVehicles;

internal sealed class GetMyVehiclesQueryValidator : AbstractValidator<GetMyVehiclesQuery>
{
    public GetMyVehiclesQueryValidator()
    {
        RuleFor(q => q.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}

