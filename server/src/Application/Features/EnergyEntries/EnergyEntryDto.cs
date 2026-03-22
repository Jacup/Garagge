using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace Application.Features.EnergyEntries;

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
    bool IsPartial,
    decimal? Cost,
    decimal? PricePerUnit,
    decimal? Consumption
);

public static class EnergyEntryExtensions
{
    public static EnergyEntryDto ToDto(this EnergyEntry entry) =>
        new(
            entry.Id,
            entry.VehicleId,
            entry.CreatedDate,
            entry.UpdatedDate,
            entry.Date,
            entry.Mileage,
            entry.Type,
            entry.EnergyUnit,
            entry.Volume,
            entry.IsPartial,
            entry.Cost,
            entry.PricePerUnit,
            0
        );
}