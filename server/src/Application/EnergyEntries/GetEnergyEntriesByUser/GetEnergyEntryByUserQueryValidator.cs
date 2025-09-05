using FluentValidation;

namespace Application.EnergyEntries.GetEnergyEntriesByUser;

internal sealed class GetEnergyEntryByUserQueryValidator : AbstractValidator<GetEnergyEntriesByUserQuery>
{
    public GetEnergyEntryByUserQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Page)
            .Must(BeValidPage)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .Must(BeValidPageSize)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.EnergyType)
            .IsInEnum()
            .When(x => x.EnergyType.HasValue)
            .WithMessage("Invalid energy type.");
    }

    private static bool BeValidPage(int page) => page > 0;

    private static bool BeValidPageSize(int pageSize) => pageSize is > 0 and <= 100;
}