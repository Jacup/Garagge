using Application.Abstractions.Messaging;
using Application.EnergyEntries.Dtos;
using Domain.Enums;

namespace Application.EnergyEntries.CreateFuelEntry;

public sealed record CreateFuelEntryCommand(
    Guid VehicleId,
    DateOnly Date,
    int Mileage,
    decimal Cost,
    decimal Volume,
    VolumeUnit Unit,
    decimal PricePerUnit) : ICommand<FuelEntryDto>;