using FluentValidation;

namespace Application.Vehicles.GetMyVehicles;

public sealed class GetMyVehiclesQueryValidator : AbstractValidator<GetMyVehiclesQuery>
{
    public GetMyVehiclesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID cannot be empty.");

        RuleFor(x => x.Page)
            .Must(BeValidPage)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SearchTerm)
            .Must(BeValidSearchTerm)
            .When(x => !string.IsNullOrEmpty(x.SearchTerm))
            .WithMessage("Search term cannot exceed 32 characters.");
    }

    private static bool BeValidPage(int page) => page > 0;
    
    private static bool BeValidPageSize(int pageSize) => pageSize is > 0 and <= 100;
    
    private static bool BeValidSearchTerm(string? searchTerm) => 
        string.IsNullOrEmpty(searchTerm) || searchTerm.Length <= 32;
}
