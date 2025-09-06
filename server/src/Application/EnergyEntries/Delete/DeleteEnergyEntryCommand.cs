using Application.Abstractions.Messaging;

namespace Application.EnergyEntries.Delete;

public sealed record DeleteEnergyEntryCommand(Guid Id, Guid VehicleId) : ICommand;