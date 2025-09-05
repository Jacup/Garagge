using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.EnergyEntries.CreateEnergyEntry;

public sealed record CreateEnergyEntryCommand(
    Guid VehicleId,
    DateOnly Date,
    int Mileage,
    EnergyType Type,
    EnergyUnit EnergyUnit,
    decimal Volume,
    decimal? Cost,
    decimal? PricePerUnit)
    : ICommand<EnergyEntryDto>;