using Application.Abstractions;
using Application.Abstractions.Services;
using Application.Core;
using Application.Core.Validators;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Vehicles.Create;

internal sealed class CreateVehicleCommandValidator : BaseEnergyTypeValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator(IDateTimeProvider dateTimeProvider, IVehicleEngineCompatibilityService compatibilityService)
        : base(compatibilityService)
    {
        const int maxLength = 64;

        RuleFor(c => c.Brand)
            .NotEmpty()
            .WithError(VehicleErrors.BrandRequired)
            .MaximumLength(maxLength)
            .WithError(VehicleErrors.BrandTooLong(maxLength));

        RuleFor(c => c.Model)
            .NotEmpty()
            .WithError(VehicleErrors.ModelRequired)
            .MaximumLength(maxLength)
            .WithError(VehicleErrors.ModelTooLong(maxLength));

        RuleFor(c => c.EngineType)
            .IsInEnum()
            .WithError(VehicleErrors.EngineTypeInvalid);

        const int firstCarManufacturedYear = 1886;

        RuleFor(c => c.ManufacturedYear)
            .GreaterThanOrEqualTo(firstCarManufacturedYear)
            .LessThanOrEqualTo(dateTimeProvider.UtcNow.Year)
            .When(c => c.ManufacturedYear.HasValue)
            .WithError(VehicleErrors.ManufacturedYearInvalid(firstCarManufacturedYear, dateTimeProvider.UtcNow.Year));

        RuleFor(c => c.Type)
            .IsInEnum()
            .When(c => c.Type.HasValue)
            .WithError(VehicleErrors.TypeInvalid);

        RuleFor(c => c.VIN)
            .Length(17)
            .When(c => c.VIN is not null)
            .WithError(VehicleErrors.VinInvalidLength);

        ConfigureEnergyTypesValidation();
    }

    private void ConfigureEnergyTypesValidation()
    {
        RuleFor(c => c.EnergyTypes)
            .Must(AreValidEnumValues)
            .WithError(VehicleErrors.EnergyTypesInvalid)
            .Must(AreUnique)
            .WithError(VehicleErrors.EnergyTypesNotUnique)
            .Must((command, energyTypes) => energyTypes == null || AreAllCompatibleWithEngine(energyTypes, command.EngineType))
            .WithMessage((command, energyTypes) => BuildIncompatibilityMessage(energyTypes, command.EngineType));
    }
}