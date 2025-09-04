using Application.Abstractions.Messaging;
using Application.EnergyEntries.Dtos;
using Domain.Enums;

namespace Application.EnergyEntries.CreateChargingEntry;

public sealed record CreateChargingEntryCommand(
    Guid VehicleId,
    DateOnly Date,
    int Mileage,
    decimal Cost,
    decimal EnergyAmount,
    EnergyUnit Unit,
    decimal PricePerUnit,
    int? ChargingDurationMinutes) : ICommand<ChargingEntryDto>;