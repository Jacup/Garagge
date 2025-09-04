using Application.Abstractions.Messaging;

namespace Application.EnergyEntries.DeleteEnergyEntry;

public sealed record DeleteEnergyEntryCommand(Guid Id, Guid VehicleId) : ICommand;