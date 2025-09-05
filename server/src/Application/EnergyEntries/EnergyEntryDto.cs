using Domain.Enums;

namespace Application.EnergyEntries;

public record EnergyEntryDto(
    Guid Id,
    Guid VehicleId,
    DateTime CreatedDate,
    DateTime UpdatedDate,
    DateOnly Date,
    int Mileage,
    EnergyType Type,
    EnergyUnit EnergyUnit,
    decimal Volume,
    decimal? Cost,
    decimal? PricePerUnit);
