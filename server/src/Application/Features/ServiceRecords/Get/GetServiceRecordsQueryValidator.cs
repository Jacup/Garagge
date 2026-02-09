using Application.Core;
using FluentValidation;

namespace Application.Features.ServiceRecords.Get;

internal sealed class GetServiceRecordsQueryValidator : AbstractValidator<GetServiceRecordsQuery>
{
    public GetServiceRecordsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithError(ServiceRecordErrors.PageInvalid);
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(ServiceRecordErrors.PageSizeInvalid);

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithError(ServiceRecordErrors.DateFilterInvalid);
        
        RuleFor(x => x.SortBy)
            .Must(x => x == null || new[] { "servicedate", "totalcost", "mileage", "title" }
                .Contains(x.ToLowerInvariant()))
            .WithError(ServiceRecordErrors.SortByInvalid);
    }
}

