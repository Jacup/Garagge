using FluentValidation;

namespace Application.ServiceRecords.Get;

internal sealed class GetServiceRecordsQueryValidator : AbstractValidator<GetServiceRecordsQuery>
{
    public GetServiceRecordsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
        
        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateFrom must be before or equal to DateTo");
        
        RuleFor(x => x.SortBy)
            .Must(x => x == null || new[] { "servicedate", "totalcost", "mileage", "title" }
                .Contains(x.ToLowerInvariant()))
            .WithMessage("Invalid sortBy value. Allowed values: servicedate, totalcost, mileage, title");
    }
}

