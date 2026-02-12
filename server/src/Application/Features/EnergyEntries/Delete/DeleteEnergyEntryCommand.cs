using Application.Abstractions.Messaging;

namespace Application.Features.EnergyEntries.Delete;

public sealed record DeleteEnergyEntryCommand(Guid Id, Guid VehicleId) : ICommand;