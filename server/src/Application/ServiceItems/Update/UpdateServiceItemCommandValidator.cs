using FluentValidation;

namespace Application.ServiceItems.Update;

internal sealed class UpdateServiceItemCommandValidator : AbstractValidator<UpdateServiceItemCommand>
{
    public UpdateServiceItemCommandValidator()
    {
        RuleFor(x => x.ServiceItemId)
            .NotEmpty()
            .WithMessage("ServiceItemId is required.");

        RuleFor(x => x.ServiceRecordId)
            .NotEmpty()
            .WithMessage("ServiceRecordId is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(128)
            .WithMessage("Name cannot exceed 128 characters.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid ServiceItemType.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price cannot be negative.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.PartNumber)
            .MaximumLength(64)
            .When(x => !string.IsNullOrEmpty(x.PartNumber))
            .WithMessage("Part number cannot exceed 64 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}