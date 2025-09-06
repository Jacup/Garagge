using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.EnergyEntries.Update;

public sealed record UpdateEnergyEntryCommand(
    Guid VehicleId,
    Guid Id,
    DateOnly Date,
    int Mileage,
    EnergyType Type,
    EnergyUnit EnergyUnit,
    decimal Volume,
    decimal? Cost,
    decimal? PricePerUnit)
    : ICommand<EnergyEntryDto>;